using System;

namespace DDC.Autotests.Framework
{
    public class Period
    {
        public string Id;
        public DateTime StartDate;
        public DateTime FinishDate;
        public string Name;

        public Period(string id, DateTime startDate, DateTime finishDate, string name)
        {
            Id = id;
            StartDate = startDate;
            FinishDate = finishDate;
            Name = name;
        }
    }
}
