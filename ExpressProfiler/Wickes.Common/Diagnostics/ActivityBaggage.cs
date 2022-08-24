using System.Collections.Generic;

namespace Wickes
{
    using Helpers;

    public class ActivityBaggage : List<KeyValuePair<string, string>>
    {
        public ActivityBaggage()
        {

        }

        public ActivityBaggage(IEnumerable<KeyValuePair<string, string>> values)
            : base(values)
        {

        }

        public override string ToString()
        {
            return this.ToLogStringAsKeyValueList();
        }
    }
}
