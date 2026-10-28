using CadetTest.Entities;
using CadetTest.Helpers;
using System.Text;

namespace CadetTest.Services
{
    public interface IDataService
    {
        string GetRandomString(int stringLength);
        List<Consent> GetRangeById(int id, int adet);
    }
    public class DataService : IDataService
    {
        private DataContext _context;

        public DataService(DataContext context)
        {
            _context = context;
        }

        #region Public Methods

        public List<Consent> GetRangeById(int id, int adet)
        {
            var cevap = _context.Consents.Where(k => k.Id >= id).OrderBy(k => k.Id).Take(adet).ToList();
            return cevap;
        }

        public string GetRandomString(int stringLength)
        {
            var sb = new StringBuilder();
            int numGuidsToConcat = (((stringLength - 1) / 32) + 1);
            for (int i = 1; i <= numGuidsToConcat; i++)
            {
                sb.Append(Guid.NewGuid().ToString("N"));
            }

            return sb.ToString(0, stringLength);
        }
        #endregion
    }
}
