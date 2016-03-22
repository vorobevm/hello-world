using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace DDC.Autotests.Framework
{
    public static class DdcRoutinesStatic
    {
        private static readonly string SapCodePrefix = ConfigurationManager.AppSettings["sapCodePrefix"];

        /// <summary>
        /// Creates a new period in DDC.Period table. Will work till Q3 2099 inclusive.
        /// </summary>
        /// <returns>Period object</returns>
        public static Period CreateNewPeriod(DbOperations dbContext)
        {
            SqlCommand sql = new SqlCommand(@"insert into ddc.period (id, StartDate, finishdate, name)
                                             select id + 1, DATEADD(day, 1, FinishDate), dateadd(month, 3, FinishDate),                                              case when Name like '20[0-9][0-9] Q[1-3]' then substring(Name, 1, 6) + cast(cast(substring(Name, 7, 1) as int) + 1 as nvarchar)
                                             when Name like '20[0-9][0-8] Q4' then substring(Name, 1, 3) + cast(cast(substring(Name, 4, 1) as int) + 1 as nvarchar) + ' Q1'
                                             when Name like '20[0-9]9 Q4' then substring(Name, 1, 2) + cast(cast(substring(Name, 3, 1) as int) + 1 as nvarchar) + '0 Q1'
                                             end as [Name] from (select top 1 * from ddc.Period order by id desc) t", dbContext.GetConnection());

            sql.ExecuteNonQuery();
            sql = new SqlCommand("select top 1 id, startdate, finishdate, name from ddc.Period order by id desc", dbContext.GetConnection());
            var reader = sql.ExecuteReader();
            if (reader.Read())
            {
                Period newPeriod = new Period(reader.GetValue(0).ToString(), DateTime.Parse(reader.GetValue(1).ToString()), DateTime.Parse(reader.GetValue(2).ToString()), reader.GetValue(3).ToString());
                return newPeriod;
            }
            else
            {
                throw new Exception("Period wasn't created!");
            }
        }

        /// <summary>
        /// Creates a new period in DDC.Period table. Will work till Q3 2099 inclusive.
        /// Also fills the fields of this.newPeriod object.
        /// </summary>
        /// <returns>DDC.Period.id value</returns>
        public static string CreateNewPeriodGetId(DbOperations dbContext)
        {
            return CreateNewPeriod(dbContext).Id;
        }

        public static Period GetPeriod(DbOperations dbContext, string periodId)
        {
            SqlCommand sql = new SqlCommand("select id, startdate, finishdate, name from ddc.Period where id = @id", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@id", periodId);
            var reader = sql.ExecuteReader();

            if (reader.Read())
            {
                return new Period(periodId, DateTime.Parse(reader.GetValue(1).ToString()), DateTime.Parse(reader.GetValue(2).ToString()), reader.GetValue(3).ToString());
            }
            else
            {
                throw new Exception("Period not found!");
            }
        }

        /// <summary>
        /// Removes period from DDC.Period table by id.
        /// </summary>
        public static void RemovePeriod(DbOperations dbContext, string id)
        {
            SqlCommand sql = new SqlCommand("delete from ddc.Period where id = @id", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@id", id);
            sql.ExecuteNonQuery();
        }

        /// <summary>
        /// Removes last created period from DDC.Period table.
        /// </summary>
        public static void RemoveLastPeriod(DbOperations dbContext)
        {
            SqlCommand sql = new SqlCommand("delete from ddc.Period where id in (select top 1 id from ddc.Period order by id desc)", dbContext.GetConnection());
            sql.ExecuteNonQuery();
        }

        /// <summary>
        /// Creates a new supplier with random title in DDC.Supplier table. 
        /// Also fills the fields of this.newSupplier object.
        /// </summary>
        public static Supplier CreateNewSupplier(DbOperations dbContext)
        {
            Supplier newSupplier = new Supplier();
            newSupplier.Title = "TEST_" + RandomString(37);
            SqlCommand sql = new SqlCommand("insert into ddc.supplier (Title, IsVirtual) values (@SupplierTitle,0)", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@SupplierTitle", newSupplier.Title);
            sql.ExecuteNonQuery();

            sql = new SqlCommand("select id from ddc.Supplier where Title = @SupplierTitle order by id desc", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@SupplierTitle", newSupplier.Title);
            var reader = sql.ExecuteReader();

            if (reader.Read())
            {
                newSupplier.Id = reader.GetValue(0).ToString();
            }
            else
            {
                throw new Exception("Supplier wasn't created!");
            }
            return newSupplier;
        }

        /// <summary>
        /// Remove supplier by Title. If no name specified - remove the supplier created during current session.
        /// </summary>
        public static void RemoveSupplier(DbOperations dbContext, string supplierTitle)
        {
            SqlCommand sql = new SqlCommand("delete from ddc.supplier where Title = @SupplierTitle", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@SupplierTitle", supplierTitle);
            sql.ExecuteNonQuery();
        }

        /// <summary>
        /// Create new Contract using provided data. Output - DDC.Contract.Id.
        /// </summary>
        public static string CreateContract(DbOperations dbContext, string supplierId, DateTime signDate, DateTime startDate, DateTime finishDate)
        {
            Random rnd = new Random();
            string contractNumber = "TEST_" + RandomString(4) + rnd.Next().ToString();
            SqlCommand sql = new SqlCommand("insert into ddc.Contract(ContractNumber, ContractSignDate, StartDate, FinishDate, CreateDate, CreateUser, EditDate, EditUser, SupplierId) Values(@ContractNumber, @SignDate, @StartDate, @FinishDate, SYSDATETIME(), 'TEST', SYSDATETIME(), 'TEST', @SupplierId)", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@ContractNumber", contractNumber);
            sql.Parameters.AddWithValue("@SignDate", signDate);
            sql.Parameters.AddWithValue("@StartDate", startDate);
            sql.Parameters.AddWithValue("@FinishDate", finishDate);
            sql.Parameters.AddWithValue("@SupplierId", supplierId);
            sql.ExecuteNonQuery();
            sql = new SqlCommand("select id from ddc.Contract where ContractNumber = @contractNumber", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@contractNumber", contractNumber);
            var reader = sql.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetValue(0).ToString();
            }
            else
            {
                throw new Exception("Supplier wasn't created or already deleted.");
            }
        }

        /// <summary>
        /// Remove Contract by DDC.Contract.Id
        /// </summary>
        public static void RemoveContractById(DbOperations dbContext, string contractId)
        {
            SqlCommand sql = new SqlCommand("delete from ddc.Contract where Id=@ContractId)", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@ContractId", contractId);
            sql.ExecuteNonQuery();
        }

        /// <summary>
        /// Create new Condition using provided data. Output - DDC.Condition.Id.
        /// </summary>
        public static string CreateCondition(DbOperations dbContext, DateTime startDate, DateTime finishDate, string contractId, DateTime createDate, Boolean allBrandsSelected, Boolean allProductsSelected)
        {
            SqlCommand sql = new SqlCommand("insert into ddc.condition (StartDate, FinishDate, AmountPercent, AmountQty, ContractId, CreateDate, CreateUser, EditDate, EditUser, Status, AllBrandsSelected, AllProductsSelected, BusinessDomainId, IsDeleted) Values(@StartDate, @FinishDate, 100, 1, @ContractId, SYSDATETIME(), 'TEST', SYSDATETIME(), 'TEST', 0, @AllBrandsSelected, @AllProductsSelected, 1, 0)", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@StartDate", startDate);
            sql.Parameters.AddWithValue("@FinishDate", finishDate);
            sql.Parameters.AddWithValue("@ContractId", contractId);
            sql.Parameters.AddWithValue("@AllBrandsSelected", allBrandsSelected);
            sql.Parameters.AddWithValue("@AllProductsSelected", allProductsSelected);
            sql.ExecuteNonQuery();

            sql = new SqlCommand("select id from ddc.Condition WHERE StartDate = @StartDate AND FinishDate = @FinishDate AND ContractId = @ContractId", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@StartDate", startDate);
            sql.Parameters.AddWithValue("@FinishDate", finishDate);
            sql.Parameters.AddWithValue("@ContractId", contractId);
            var reader = sql.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetValue(0).ToString();
            }
            else
            {
                throw new Exception("Condition wasn't created or already deleted.");
            }
        }

        /// <summary>
        /// Remove Condition by DDC.Condition.Id
        /// </summary>
        public static void RemoveConditionById(DbOperations dbContext, string conditionId)
        {
            SqlCommand sql = new SqlCommand("delete ddc.Condition where id = @ConditionId", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@ConditionId", conditionId);
            sql.ExecuteNonQuery();
        }

        /// <summary>
        /// Enumeration of possible Rule types in DDC.RuleOfCalculating table.
        /// </summary>
        public enum RuleType
        {
            Article = 1,
            ProductGroup = 2,
            Department = 4,
            BusinessDomain = 8,
            Brand = 16,
            Distributor = 32,
            InvoiceRecipient = 96
        }

        /// <summary>
        /// Base method for creating a row in RuleOfCalculating table
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="type">Taken from RuleType enumeration</param>
        /// <param name="conditionId">DDC.Condition.id value</param>
        /// <param name="linkedEntityId"></param>
        /// <returns>DDC.RuleOfCalculation.id</returns>
        private static string CreateRuleOfCalc(DbOperations dbContext, RuleType type, string conditionId, string linkedEntityId)
        {
            SqlCommand sql = new SqlCommand("insert into ddc.RuleOfCalculating (Type, ConditionId,LinkedEntityId)" +
                                            "Values(@Type,@ConditionId,@LinkedEntityId)", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@Type", (int)type);
            sql.Parameters.AddWithValue("@ConditionId", conditionId);
            sql.Parameters.AddWithValue("@LinkedEntityId", linkedEntityId);
            sql.ExecuteNonQuery();

            sql = new SqlCommand("select top 1 id from ddc.RuleOfCalculating order by id desc", dbContext.GetConnection());
            var reader = sql.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetValue(0).ToString();
            }
            else
            {
                throw new Exception("Rule wasn't created.");
            }
        }

        /// <summary>
        /// Remove DDC.RuleOfCalculating by Type, ConditionId, LinkedEntityId
        /// </summary>
        public static void RemoveRuleOfCalc(DbOperations dbContext, RuleType type, string conditionId, string linkedEntityId)
        {
            SqlCommand sql = new SqlCommand("delete from ddc.RuleOfCalculating where Type = @Type AND ConditionId = @ConditionId AND LinkedEntityId = @LinkedEntityId", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@Type", (int)type);
            sql.Parameters.AddWithValue("@ConditionId", conditionId);
            sql.Parameters.AddWithValue("@LinkedEntityId", linkedEntityId);
            sql.ExecuteNonQuery();
        }

        /// <summary>
        /// Remove DDC.RuleOfCalculating by Id
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="id"></param>
        public static void RemoveRuleOfCalc(DbOperations dbContext, string id)
        {
            SqlCommand sql = new SqlCommand("delete from ddc.RuleOfCalculating where Id = @id", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@id", id);
            sql.ExecuteNonQuery();
        }

        public static string CreateRuleOfCalcArticle(DbOperations dbContext, string conditionId, string articleId)
        {
            return CreateRuleOfCalc(dbContext, RuleType.Article, conditionId, articleId);
        }

        public static string CreateRuleOfCalcProductGroup(DbOperations dbContext, string conditionId, string productGroupId)
        {
            return CreateRuleOfCalc(dbContext, RuleType.ProductGroup, conditionId, productGroupId);
        }

        public static string CreateRuleOfCalcDepartment(DbOperations dbContext, string conditionId, string departmentId)
        {
            return CreateRuleOfCalc(dbContext, RuleType.Department, conditionId, departmentId);
        }

        public static string CreateRuleOfCalcBusinessDomain(DbOperations dbContext, string conditionId, string businessDomainId)
        {
            return CreateRuleOfCalc(dbContext, RuleType.BusinessDomain, conditionId, businessDomainId);
        }

        public static string CreateRuleOfCalcBrand(DbOperations dbContext, string conditionId, string brandId)
        {
            return CreateRuleOfCalc(dbContext, RuleType.Brand, conditionId, brandId);
        }

        public static string CreateRuleOfCalcDistributor(DbOperations dbContext, string conditionId, string supplierId)
        {
            return CreateRuleOfCalc(dbContext, RuleType.Distributor, conditionId, supplierId);
        }

        public static string CreateRuleOfCalcInvoiceRecipient(DbOperations dbContext, string conditionId, string invoiceRecipientId)
        {
            return CreateRuleOfCalc(dbContext, RuleType.InvoiceRecipient, conditionId, invoiceRecipientId);
        }

        /// <summary>
        /// Method to create a record in DDC.GoodsRecord table
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="sapCode">Store SAP_CODE</param>
        /// <param name="posNo"></param>
        /// <param name="buchNo"></param>
        /// <param name="preis">Price</param>
        /// <param name="buchSubTyp"></param>
        /// <param name="artNo">Article number</param>
        /// <param name="wareneingang">Date of operation</param>
        /// <param name="menge">Number of articles</param>
        /// <param name="supplierId"></param>
        /// <param name="bestellNo"></param>
        /// <param name="bsPos"></param>
        /// <param name="waehrung">Currency in 3-char ISO code</param>
        /// <returns></returns>
        public static void CreateGoodsRecord(DbOperations dbContext, string sapCode, string posNo, string buchNo, string preis, string buchSubTyp, string artNo, DateTime wareneingang, string menge, string supplierId, string bestellNo, string bsPos, string waehrung = "SEK")
        {
            SqlCommand sql = new SqlCommand("insert into ddc.GoodsRecord (SAP_CODE, POS_NO, BUCH_NO, PREIS, BUCH_SUB_TYP, ART_NO, WARENEINGANG, MENGE, LIEF_NO, BESTELL_NO, BS_POS, ROWVER, WAEHRUNG) Values(@SAP_CODE, @POS_NO, @BUCH_NO, @PREIS, @BUCH_SUB_TYP, @ART_NO, cast (@WARENEINGANG as Date), @MENGE, @SupplierId, @BESTELL_NO, @BS_POS,'TEST', @WAEHRUNG)", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@SAP_CODE", sapCode);
            sql.Parameters.AddWithValue("@POS_NO", posNo);
            sql.Parameters.AddWithValue("@BUCH_NO", buchNo);
            sql.Parameters.AddWithValue("@PREIS", preis);
            sql.Parameters.AddWithValue("@BUCH_SUB_TYP", buchSubTyp);
            sql.Parameters.AddWithValue("@ART_NO", artNo);
            sql.Parameters.AddWithValue("@WARENEINGANG", wareneingang);
            sql.Parameters.AddWithValue("@MENGE", menge);
            sql.Parameters.AddWithValue("@SupplierId", supplierId);
            sql.Parameters.AddWithValue("@BESTELL_NO", bestellNo);
            sql.Parameters.AddWithValue("@BS_POS", bsPos);
            sql.Parameters.AddWithValue("@WAEHRUNG", waehrung);
            sql.ExecuteNonQuery();
        }

        /// <summary>
        /// Check expected test result in DDC.PeriodCalculation table
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="periodId"></param>
        /// <param name="conditionId"></param>
        /// <param name="status"></param>
        /// <param name="discount"></param>
        /// <param name="sapCode">Store SAP_CODE</param>
        /// <param name="artNo">Article number</param>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        public static Boolean CheckExpectedPeriodCalc(DbOperations dbContext, string periodId, string conditionId, string status, string discount, string sapCode, string artNo, string supplierId)
        {
            // TODO Need to be tested on real data. Maybe, not count(*) should be used to check the results.
            SqlCommand sql = new SqlCommand("select count(*) from ddc.PeriodCalculation" +
                                            " where PeriodId= @PeriodId AND ConditionId= @ConditionId AND Status= @Status AND Discount=@Discount AND SAP_CODE=@SAP_CODE AND ART_NO = @ART_NO AND LIEF_NO=@SupplierId", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@PeriodId", periodId);
            sql.Parameters.AddWithValue("@ConditionId", conditionId);
            sql.Parameters.AddWithValue("@Status", status);
            sql.Parameters.AddWithValue("@Discount", discount);
            sql.Parameters.AddWithValue("@SAP_CODE", sapCode);
            sql.Parameters.AddWithValue("@ART_NO", artNo);
            sql.Parameters.AddWithValue("@SupplierId", supplierId);
            var reader = sql.ExecuteReader();
            if (reader.Read())
            {
                if (Int32.Parse(reader.GetValue(0).ToString()) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                throw new Exception();
            }

        }

        /// <summary>
        /// Create new CustomerOrder record
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="sapCode"></param>
        /// <param name="bestellNo"></param>
        /// <param name="bsPos"></param>
        /// <param name="artNo"></param>
        /// <param name="supplierId"></param>
        /// <param name="menge"></param>
        public static void CreateCustomerOrder(DbOperations dbContext, string sapCode, string bestellNo, string bsPos, string artNo,
            string supplierId, string menge)
        {
            SqlCommand sql = new SqlCommand("INSERT INTO ddc.CustomerOrder (SAP_CODE, BESTELL_NO, BS_POS, ART_NO, LIEF_NO, MENGE) VALUES (@SAP_CODE, @BESTELL_NO, @BS_POS, @ART_NO, @LIEF_NO, @MENGE)", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@SAP_CODE", sapCode);
            sql.Parameters.AddWithValue("@BESTELL_NO", bestellNo);
            sql.Parameters.AddWithValue("@BS_POS", bsPos);
            sql.Parameters.AddWithValue("@ART_NO", artNo);
            sql.Parameters.AddWithValue("@LIEF_NO", supplierId);
            sql.Parameters.AddWithValue("@MENGE", menge);
            sql.ExecuteNonQuery();
        }

        /// <summary>
        /// Remove CustomerOrder record by all attributes
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="sapCode"></param>
        /// <param name="bestellNo"></param>
        /// <param name="bsPos"></param>
        /// <param name="artNo"></param>
        /// <param name="supplierId"></param>
        /// <param name="menge"></param>
        public static void RemoveCustomerOrder(DbOperations dbContext, string sapCode, string bestellNo, string bsPos,
            string artNo,
            string supplierId, string menge)
        {
            SqlCommand sql = new SqlCommand("DELETE FROM ddc.CustomerOrder WHERE SAP_CODE = @SAP_CODE AND BESTELL_NO = @BESTELL_NO AND BS_POS=@BS_POS AND ART_NO=@ART_NO AND LIEF_NO=@LIEF_NO AND MENGE=@MENGE", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@SAP_CODE", sapCode);
            sql.Parameters.AddWithValue("@BESTELL_NO", bestellNo);
            sql.Parameters.AddWithValue("@BS_POS", bsPos);
            sql.Parameters.AddWithValue("@ART_NO", artNo);
            sql.Parameters.AddWithValue("@LIEF_NO", supplierId);
            sql.Parameters.AddWithValue("@MENGE", menge);
            sql.ExecuteNonQuery();
        }

        /// <summary>
        /// Create new Product and add it to ddc.Product table
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="name">Can be empty</param>
        /// <param name="departmentId"></param>
        /// <param name="productGroupId"></param>
        /// <param name="brandId"></param>
        /// <returns>Product.Id (ART_NO)</returns>
        public static string CreateNewProduct(DbOperations dbContext, string name, int departmentId,
            int productGroupId, int brandId)
        {
            string prodId = GetNewArticle(dbContext);
            if (name == "")
            {
                name = "TESTARTICLE_" + RandomString(10);
            }
            SqlCommand sql = new SqlCommand("INSERT INTO ddc.Product (Id, Name, DepartmentId, ProductGroupId, BrandId) values (@id, @Name, @DepartmentId, @ProductGroupId, @BrandId)", dbContext.GetConnection());
            sql.Parameters.AddWithValue("@id", prodId);
            sql.Parameters.AddWithValue("@Name", name);
            sql.Parameters.AddWithValue("@DepartmentId", departmentId);
            sql.Parameters.AddWithValue("@ProductGroupId", productGroupId);
            sql.Parameters.AddWithValue("@BrandId", brandId);
            sql.ExecuteNonQuery();
            return prodId;
        }

        /// <summary>
        /// Get (new) article number that doesn't exist in GoodsRecord.ART_NO.
        /// </summary>
        public static string GetNewArticle(DbOperations dbContext)
        {
            Random rnd = new Random();
            int newArt;
            SqlDataReader reader;
            do
            {
                newArt = rnd.Next(10000, 999999);
                SqlCommand sql = new SqlCommand("select * from ddc.GoodsRecord where ART_NO = @ART_NO", dbContext.GetConnection());
                sql.Parameters.AddWithValue("@ART_NO", newArt.ToString());
                reader = sql.ExecuteReader();
            }
            while (reader.HasRows);
            return newArt.ToString();
        }

        /// <summary>
        /// Get (new) store SAP_CODE that doesn't exist in GoodsRecord.SAP_CODE.
        /// </summary>
        public static string GetNewStore(DbOperations dbContext)
        {
            Random rnd = new Random();
            SqlDataReader reader;
            string store;
            do
            {
                store = SapCodePrefix + rnd.Next(100, 999).ToString();
                SqlCommand sql = new SqlCommand("select * from ddc.GoodsRecord where SAP_CODE = @store", dbContext.GetConnection());
                sql.Parameters.AddWithValue("@store", store);
                reader = sql.ExecuteReader();
            }
            while (reader.HasRows);
            return store;
        }

        /// <summary>
        /// Get (new) store SAP_CODE that doesn't exist in GoodsRecord.SAP_CODE and not equal to provided ExcludedStore code
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="excludedStore"></param>
        /// <returns></returns>
        public static string GetNewStore(DbOperations dbContext, string excludedStore)
        {
            string store;
            do
            {
                store = GetNewStore(dbContext);
            }
            while (store == excludedStore);
            return store;
        }

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }
    }
}