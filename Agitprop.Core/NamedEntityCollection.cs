using System.Text.Json.Serialization;

namespace Agitprop.Core;

public record NamedEntityCollection
{
        [JsonPropertyName("PER")]
        public List<string> PER { get; set; } = [];

        [JsonPropertyName("LOC")]
        public List<string> LOC { get; set; } = [];

        [JsonPropertyName("ORG")]
        public List<string> ORG { get; set; } = [];

        [JsonPropertyName("MISC")]
        public List<string> MISC { get; set; } = [];

        public List<string> All
        {
                get
                {
                        List<string> all = [];
                        all.AddRange(PER);
                        all.AddRange(LOC);
                        all.AddRange(ORG);
                        all.AddRange(MISC);
                        return all;
                }
        }
}
