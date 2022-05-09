using System;
using System.Text.Json;

namespace LindormStudio
{
    public class LindormResponse
    {
        public string[] Columns { get; set; } = Array.Empty<string>();
        public string[] Metadatas { get; set; } = Array.Empty<string>();
        public JsonElement[][] Rows { get; set; } = Array.Empty<JsonElement[]>();
    }
}
