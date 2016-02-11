select id+1, DATEADD(day,1, FinishDate), dateadd(month, 3, FinishDate), 
  case when Name like '20[0-9][0-9] Q[1-3]' then substring(Name,1,6)+cast(cast(substring(Name, 7,1) as int)+1 as nvarchar)
	   when Name like '20[0-9][0-8] Q4' then substring(Name,1,3)+cast(cast(substring(Name, 4,1) as int)+1 as nvarchar)+' Q1'
	   when Name like '20[0-9]9 Q4' then substring(Name,1,2)+cast(cast(substring(Name, 3,1) as int)+1 as nvarchar)+'0 Q1'
  end as [Name]
  from (select top 5 * from ddc.ddc.Period order by id desc
  union
  select 31, cast('2015-04-01 00:00:00.000' as datetime), cast('2015-04-01 00:00:00.000' as datetime), '2019 Q4'
  ) t
