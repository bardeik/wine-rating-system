using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace WineRatingApp.Models
{
    public static class GlobalLists
    {
        public static List<SelectListItem> WineIdJudgeSelectList(int selectedValue)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            ApplicationDbContext db = new ApplicationDbContext();
            var wines = db.Wines.ToList().OrderBy(x => x.RatingName);
            foreach (var wine in wines)
            {
                selectListItems.Add(new SelectListItem { Text = wine.RatingName, Value = wine.WineId.ToString(), Selected = wine.WineId == selectedValue });
            }
            return selectListItems;
        }

        public static List<SelectListItem> WineRatingVisualitySelectList(int selectedValue)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            foreach (WineRatingVisuality element in Enum.GetValues(typeof(WineRatingVisuality)).Cast<WineRatingVisuality>().ToList())
            {
                switch (element)
                {
                    case WineRatingVisuality.Feilbeheftet:
                        selectListItems.Add(new SelectListItem { Text = "Feilbeheftet 0", Value = "0", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingVisuality.UfullstendigEllerMangelfull:
                        selectListItems.Add(new SelectListItem { Text = "Ufullstendig eller Mangelfull 1", Value = "1", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingVisuality.Akseptabel:
                        selectListItems.Add(new SelectListItem { Text = "Akseptabel 2", Value = "2", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingVisuality.SærligeKvaliteter:
                        selectListItems.Add(new SelectListItem { Text = "SærligeKvaliteter 3", Value = "3", Selected = (int)element == selectedValue });
                        break;
                }
            }
            return selectListItems;
        }
        public static List<SelectListItem> WineRatingNoseSelectList(int selectedValue)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            foreach (WineRatingNose element in Enum.GetValues(typeof(WineRatingNose)).Cast<WineRatingNose>().ToList())
            {
                switch (element)
                {
                    case WineRatingNose.Feilbeheftet:
                        selectListItems.Add(new SelectListItem { Text = "Feilbeheftet 0", Value = "0", Selected = (int)element == selectedValue});
                        break;
                    case WineRatingNose.UfullstendigEllermangelfull:
                        selectListItems.Add(new SelectListItem { Text = "Ufullstendig eller Mangelfull 1", Value = "1", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingNose.Akseptabel:
                        selectListItems.Add(new SelectListItem { Text = "Akseptabel 2", Value = "2", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingNose.SærligeKvaliteter3:
                        selectListItems.Add(new SelectListItem { Text = "Særlige Kvaliteter 3", Value = "3", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingNose.SærligeKvaliteter4:
                        selectListItems.Add(new SelectListItem { Text = "Særlige Kvaliteter 4", Value = "4", Selected = (int)element == selectedValue });
                        break;
                }
            }
            return selectListItems;
        }
        public static List<SelectListItem> WineRatingTasteSelectList(int selectedValue)
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            foreach (WineRatingApp.Models.WineRatingTaste element in Enum.GetValues(typeof(WineRatingApp.Models.WineRatingTaste)).Cast<WineRatingApp.Models.WineRatingTaste>().ToList())
            {
                switch (element)
                {
                    case WineRatingTaste.Feilbeheftet0:
                        selectListItems.Add(new SelectListItem { Text = "Feilbeheftet 0", Value = "0", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.Feilbeheftet1:
                        selectListItems.Add(new SelectListItem { Text = "Feilbeheftet 1", Value = "1", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.UfullstendigEllerMangelfull2:
                        selectListItems.Add(new SelectListItem { Text = "Ufullstendig eller Mangelfull 2", Value = "2", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.UfullstendigEllerMangelfull3:
                        selectListItems.Add(new SelectListItem { Text = "Ufullstendig eller Mangelfull 3", Value = "3", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.UfullstendigEllerMangelfull4:
                        selectListItems.Add(new SelectListItem { Text = "Ufullstendig eller Mangelfull 4", Value = "4", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.UfullstendigEllerMangelfull5:
                        selectListItems.Add(new SelectListItem { Text = "Ufullstendig eller Mangelfull 5", Value = "5", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.Akseptabel6:
                        selectListItems.Add(new SelectListItem { Text = "Akseptabel 6", Value = "6", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.Akseptabel7:
                        selectListItems.Add(new SelectListItem { Text = "Akseptabel 7", Value = "7", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.Balansert8:
                        selectListItems.Add(new SelectListItem { Text = "Balansert 8", Value = "8", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.Balansert9:
                        selectListItems.Add(new SelectListItem { Text = "Balansert 9", Value = "9", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.SærligVellagetVin10:
                        selectListItems.Add(new SelectListItem { Text = "Særlig Vellaget 10", Value = "10", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.SærligVellagetVin11:
                        selectListItems.Add(new SelectListItem { Text = "Særlig Vellaget 11", Value = "11", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.SærligVellagetVin12:
                        selectListItems.Add(new SelectListItem { Text = "Særlig Vellaget 12", Value = "12", Selected = (int)element == selectedValue });
                        break;
                    case WineRatingTaste.SærligVellagetVin13:
                        selectListItems.Add(new SelectListItem { Text = "Særlig Vellaget 13", Value = "13", Selected = (int)element == selectedValue });
                        break;
                }
            }
            return selectListItems;
        }
    }

}