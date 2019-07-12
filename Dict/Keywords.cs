using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hors.Dict
{
    public class Keywords
    {
        public static readonly string[] After            = {"через"};
        public static readonly string[] AfterPostfix     = {"спустя"};
        public static readonly string[] PreviousPostfix  = {"назад"};
        public static readonly string[] Next             = {"следующий", "будущий"};
        public static readonly string[] Previous         = {"прошлый", "прошедший", "предыдущий"};
        public static readonly string[] Current          = {"этот", "текущий", "нынешний"};
        public static readonly string[] CurrentNext      = {"ближайший", "грядущий"};

        public static readonly string[] Today            = {"сегодня"};
        public static readonly string[] Tomorrow         = {"завтра"};
        public static readonly string[] AfterTomorrow    = {"послезавтра"};
        public static readonly string[] Yesterday        = {"вчера"};
        public static readonly string[] BeforeYesterday  = {"позавчера"};

        public static readonly string[] Holiday          = {"выходной"};

        public static readonly string[] Second           = {"секунда", "сек"};
        public static readonly string[] Minute           = {"минута", "мин"};
        public static readonly string[] Hour             = {"час", "ч"};
        
        public static readonly string[] Day              = {"день"};
        public static readonly string[] Week             = {"неделя"};
        public static readonly string[] Month            = {"месяц", "мес"};
        public static readonly string[] Year             = {"год"};

        public static readonly string[] Noon             = {"полдень"};
        public static readonly string[] Morning          = {"утро"};
        public static readonly string[] Evening          = {"вечер"};
        public static readonly string[] Night            = {"ночь"};

        public static readonly string[] Half             = {"половина", "пол"};
        public static readonly string[] Quarter          = {"четверть"};

        public static readonly string[] DayInMonth       = {"число"};
        public static readonly string[] January          = {"январь", "янв"};
        public static readonly string[] February         = {"февраль", "фев"};
        public static readonly string[] March            = {"март", "мар"};
        public static readonly string[] April            = {"апрель", "апр"};
        public static readonly string[] May              = {"май", "мая"};
        public static readonly string[] June             = {"июнь", "июн"};
        public static readonly string[] July             = {"июль", "июл"};
        public static readonly string[] August           = {"август", "авг"};
        public static readonly string[] September        = {"сентябрь", "сен", "сент"};
        public static readonly string[] October          = {"октябрь", "окт"};
        public static readonly string[] November         = {"ноябрь", "ноя", "нояб"};
        public static readonly string[] December         = {"декабрь", "дек"};

        public static readonly string[] Monday           = {"понедельник", "пн"};
        public static readonly string[] Tuesday          = {"вторник", "вт"};
        public static readonly string[] Wednesday        = {"среда", "ср"};
        public static readonly string[] Thursday         = {"четверг", "чт"};
        public static readonly string[] Friday           = {"пятница", "пт"};
        public static readonly string[] Saturday         = {"суббота", "сб"};
        public static readonly string[] Sunday           = {"воскресенье", "вс"};
        
        public static readonly string[] DaytimeDay       = {"днём", "днем"};
        public static readonly string[] TimeFrom         = {"в", "с"};
        public static readonly string[] TimeTo           = {"до", "по"};
        public static readonly string[] TimeOn           = {"на"};

        public static List<string[]> Months()
        {
            return new List<string[]>
            {
                January,
                February,
                March,
                April,
                May,
                June,
                July,
                August,
                September,
                October,
                November,
                December
            };
        }

        public static List<string[]> DaysOfWeek()
        {
            return new List<string[]>
            {
                Monday,
                Tuesday,
                Wednesday,
                Thursday,
                Friday,
                Saturday,
                Sunday
            };
        }
        

        public List<string> AllValues()
        {
            var values = new List<string>();
            GetType()
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .ToList()
                .ForEach(f =>
                {
                    var words = (string[]) f.GetValue(null);
                    words.ToList().ForEach(values.Add);
                });
            
            return values;
        }
    }
}