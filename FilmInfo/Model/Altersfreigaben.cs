using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FilmInfo.Model
{
    public static class Altersfreigaben
    {
        public static async Task<int> getFskAsync(int TMDb_ID)
        {
            return await getFskAsync(TMDb_ID.ToString());
        }

        public static async Task<int> getFskAsync(string IMDb_ID)
        {
            var query = $"https://altersfreigaben.de/api2/s/{IMDb_ID}/de";

            string response;
            int fsk;
            var client = new HttpClient();

            try
            {
                response = await client.GetStringAsync(query);
                fsk = Convert.ToInt32(response);
                return fsk;
            }
            catch (Exception)
            {
                return -1;
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}
