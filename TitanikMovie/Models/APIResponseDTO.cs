using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectMovie.Models
{
    internal class APIResponseDTO
    {




        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("movie")]
        public MovieDTO Movie { get; set; }
    }
}

