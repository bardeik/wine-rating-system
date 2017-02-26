using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WineApp.Models;

namespace WineApp.Data
{
    public interface IWineRepository
    {
        IList<Wine> GetAllWines();
        IList<Wine> GetAllWinesFromProducer(int producerId);
        Wine GetWineById(int id);
        int AddWine(Wine wine);
        void DeleteWine(int id);
    }
}
