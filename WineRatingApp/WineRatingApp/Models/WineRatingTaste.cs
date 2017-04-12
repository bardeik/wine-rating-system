using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace WineRatingApp.Models
{
    public enum WineRatingTaste
    {
        Feilbeheftet0 = 0,
        Feilbeheftet1 = 1,
        UfullstendigEllerMangelfull2 = 2,
        UfullstendigEllerMangelfull3 = 3,
        UfullstendigEllerMangelfull4 = 4,
        UfullstendigEllerMangelfull5 = 5,
        Akseptabel6 = 6,
        Akseptabel7 = 7,
        Balansert8 = 8,
        Balansert9 = 9,
        SærligVellagetVin10 = 10,
        SærligVellagetVin11 = 11,
        SærligVellagetVin12 = 12,
        SærligVellagetVin13 = 13
    }

    public static class GlobalLists {
        public static List<SelectListItem> WineRatingTasteSelectList() {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            foreach (WineRatingApp.Models.WineRatingTaste element in Enum.GetValues(typeof(WineRatingApp.Models.WineRatingTaste)).Cast<WineRatingApp.Models.WineRatingTaste>().ToList())
            {
                switch (element) {
                    case WineRatingTaste.Feilbeheftet0:
                        selectListItems.Add(new SelectListItem { Text = "Feilbeheftet 0", Value = "0" });
                    break;
                    case WineRatingTaste.Feilbeheftet1:
                        selectListItems.Add(new SelectListItem { Text = "Feilbeheftet 1", Value = "1" });
                        break;
                    case WineRatingTaste.UfullstendigEllerMangelfull2:
                        selectListItems.Add(new SelectListItem { Text = "Ufullstendig eller Mangelfull 2", Value = "2" });
                        break;
                    case WineRatingTaste.UfullstendigEllerMangelfull3:
                        selectListItems.Add(new SelectListItem { Text = "Ufullstendig eller Mangelfull 3", Value = "3" });
                        break;
                    case WineRatingTaste.UfullstendigEllerMangelfull4:
                        selectListItems.Add(new SelectListItem { Text = "Ufullstendig eller Mangelfull 4", Value = "4" });
                        break;
                    case WineRatingTaste.UfullstendigEllerMangelfull5:
                        selectListItems.Add(new SelectListItem { Text = "Ufullstendig eller Mangelfull 5", Value = "5" });
                        break;
                    case WineRatingTaste.Akseptabel6:
                        selectListItems.Add(new SelectListItem { Text = "Akseptabel 6", Value = "6" });
                        break;
                    case WineRatingTaste.Akseptabel7:
                        selectListItems.Add(new SelectListItem { Text = "Akseptabel 7", Value = "7" });
                        break;
                    case WineRatingTaste.Balansert8:
                        selectListItems.Add(new SelectListItem { Text = "Balansert 8", Value = "8" });
                        break;
                    case WineRatingTaste.Balansert9:
                        selectListItems.Add(new SelectListItem { Text = "Balansert 9", Value = "9" });
                        break;
                    case WineRatingTaste.SærligVellagetVin10:
                        selectListItems.Add(new SelectListItem { Text = "Særlig Vellaget 10", Value = "10" });
                        break;
                    case WineRatingTaste.SærligVellagetVin11:
                        selectListItems.Add(new SelectListItem { Text = "Særlig Vellaget 11", Value = "11" });
                        break;
                    case WineRatingTaste.SærligVellagetVin12:
                        selectListItems.Add(new SelectListItem { Text = "Særlig Vellaget 12", Value = "12" });
                        break;
                    case WineRatingTaste.SærligVellagetVin13:
                        selectListItems.Add(new SelectListItem { Text = "Særlig Vellaget 13", Value = "13" });
                        break;
                }
            }
            return selectListItems;
        }
    }
}
