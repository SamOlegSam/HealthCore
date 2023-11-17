using System;
using HealthCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Web;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.Office.Interop.Word;
using NPOI.XWPF.UserModel;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.Model;
using System.Text.Json;
//using NPOI.Util;
using System.Xml;
using System.Configuration;
using System.Collections.Specialized;

namespace HealthCore.Controllers
{
    public class md5
    {
        public static string hashPassword(string password)
        {
            MD5 md5 = MD5.Create();
            byte[] b = Encoding.ASCII.GetBytes(password);
            byte[] hash = md5.ComputeHash(b);

            StringBuilder sb = new StringBuilder();
            foreach (var a in hash)
            {
                sb.Append(a.ToString("X2"));
            }
            return Convert.ToString(sb);
        }
    }
    [Authorize]
    public class HomeController : Controller
    {
        public HealthCoreContext db;
        public HomeController(HealthCoreContext context)
        {
            db = context;
        }

        //private readonly IWebHostEnvironment _appEnvironment;
        //public HomeController(IWebHostEnvironment appEnvironment)
        //{
        //    _appEnvironment = appEnvironment;
        //}

        public async Task<IActionResult> LogOut()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Access");
        }

        public IActionResult Index()
        {
            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Dogovor> ListDog = new List<Dogovor>();
            ListDog = db.Dogovor.Include(t => t.Sanatorium).Where(r => r.PriznakClose != "Закрыт" && DateTime.Now >= r.DateEnd && r.Primech == Convert.ToString(UsFil.Employee.FilialId)).ToList();

            //Получим список заявлений неоплаченных и с датой до 30 дней даты С

            //-----------------------------------------------------------------            
            List <Zayavlenie > LD = new List<Zayavlenie>();
            LD = db.Zayavlenie.Include(u =>u.Sanatorium.Dogovor).Include(y =>y.Employee.Filial).Where(r => r.Employee.FilialId == UsFil.Employee.FilialId && r.PriznakOplata == 0).ToList();
            var koldays = LD.Where(t => (Convert.ToDateTime(t.S) - DateTime.Now).TotalDays <=30);
            ViewBag.Zay30 =koldays;
            //--------------------------------------
            //List<Zayavlenie> LD = new List<Zayavlenie>();
            //LD = db.Zayavlenie.Include(u =>u.Sanatorium.Dogovor).Where(r => r.Sanatorium.Dogovor.PriznakClose != "Закрыт" && r.Sanatorium.Dogovor. == Convert.ToString(UsFil.Employee.FilialId)).ToList();
            //var DOG = LD.GroupBy(h => h.Sanatorium.Name).ToList();
            //ViewBag.DOG = DOG;
            //--------------------------------------

            return View(ListDog);
        }

        public IActionResult ReportPrint()
        {
            var phone = HttpContext.User.FindAll(ClaimTypes.Role);
            List<Fillial> ListF = new List<Fillial>();
            //ListF = db.Fillial.ToList();
            foreach (var f in phone)
            {
                foreach (var item in db.Fillial.ToList())
                {
                    if (item.Id == Convert.ToInt32(f.Value))
                    {
                        ListF.Add(item);
                    }
                }
            }

            ViewBag.ListF = ListF;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //Эта вся хуета нужна для вывода суммы прописью в докладных записках!!!
        public class RusNumber
        {
            private static string[] hunds =
            {
            "", "сто ", "двести ", "триста ", "четыреста ",
            "пятьсот ", "шестьсот ", "семьсот ", "восемьсот ", "девятьсот "
        };

            private static string[] tens =
            {
            "", "десять ", "двадцать ", "тридцать ", "сорок ", "пятьдесят ",
            "шестьдесят ", "семьдесят ", "восемьдесят ", "девяносто "
        };

            public static string Str(int val, bool male, string one, string two, string five)
            {
                string[] frac20 =
                {
                "", "один ", "два ", "три ", "четыре ", "пять ", "шесть ",
                "семь ", "восемь ", "девять ", "десять ", "одиннадцать ",
                "двенадцать ", "тринадцать ", "четырнадцать ", "пятнадцать ",
                "шестнадцать ", "семнадцать ", "восемнадцать ", "девятнадцать "
            };

                int num = val % 1000;
                if (0 == num) return "";
                if (num < 0) throw new ArgumentOutOfRangeException("val", "Параметр не может быть отрицательным");
                if (!male)
                {
                    frac20[1] = "одна ";
                    frac20[2] = "две ";
                }

                StringBuilder r = new StringBuilder(hunds[num / 100]);

                if (num % 100 < 20)
                {
                    r.Append(frac20[num % 100]);
                }
                else
                {
                    r.Append(tens[num % 100 / 10]);
                    r.Append(frac20[num % 10]);
                }

                r.Append(Case(num, one, two, five));

                if (r.Length != 0) r.Append(" ");
                return r.ToString();
            }

            public static string Case(int val, string one, string two, string five)
            {
                int t = (val % 100 > 20) ? val % 10 : val % 20;

                switch (t)
                {
                    case 1: return one;
                    case 2: case 3: case 4: return two;
                    default: return five;
                }
            }
        };

        struct CurrencyInfo
        {
            public bool male;
            public string seniorOne, seniorTwo, seniorFive;
            public string juniorOne, juniorTwo, juniorFive;
        };

        public class RusCurrencySectionHandler : IConfigurationSectionHandler
        {
            public object Create(object parent, object configContext, XmlNode section)
            {
                foreach (XmlNode curr in section.ChildNodes)
                {
                    if (curr.Name == "currency")
                    {
                        XmlNode senior = curr["senior"];
                        XmlNode junior = curr["junior"];
                        RusCurrency.Register(
                            curr.Attributes["code"].InnerText,
                            (curr.Attributes["male"].InnerText == "1"),
                            senior.Attributes["one"].InnerText,
                            senior.Attributes["two"].InnerText,
                            senior.Attributes["five"].InnerText,
                            junior.Attributes["one"].InnerText,
                            junior.Attributes["two"].InnerText,
                            junior.Attributes["five"].InnerText);
                    }
                }
                return null;
            }
        };

        public class RusCurrency
        {
            private static HybridDictionary currencies = new HybridDictionary();

            static RusCurrency()
            {
                Register("RUR", true, "рубль", "рубля", "рублей", "копейка", "копейки", "копеек");
                Register("EUR", true, "евро", "евро", "евро", "евроцент", "евроцента", "евроцентов");
                Register("USD", true, "доллар", "доллара", "долларов", "цент", "цента", "центов");
                ConfigurationSettings.GetConfig("currency-names");
            }

            public static void Register(string currency, bool male,
                string seniorOne, string seniorTwo, string seniorFive,
                string juniorOne, string juniorTwo, string juniorFive)
            {
                CurrencyInfo info;
                info.male = male;
                info.seniorOne = seniorOne; info.seniorTwo = seniorTwo; info.seniorFive = seniorFive;
                info.juniorOne = juniorOne; info.juniorTwo = juniorTwo; info.juniorFive = juniorFive;
                currencies.Add(currency, info);
            }

            public static string Str(double val)
            {
                return Str(val, "RUR");
            }

            public static string Str(double val, string currency)
            {
                if (!currencies.Contains(currency))
                    throw new ArgumentOutOfRangeException("currency", "Валюта \"" + currency + "\" не зарегистрирована");

                CurrencyInfo info = (CurrencyInfo)currencies[currency];
                return Str(val, info.male,
                    info.seniorOne, info.seniorTwo, info.seniorFive,
                    info.juniorOne, info.juniorTwo, info.juniorFive);
            }

            public static string Str(double val, bool male,
                string seniorOne, string seniorTwo, string seniorFive,
                string juniorOne, string juniorTwo, string juniorFive)
            {
                bool minus = false;
                if (val < 0) { val = -val; minus = true; }

                int n = (int)val;
                int remainder = (int)((val - n + 0.005) * 100);

                StringBuilder r = new StringBuilder();

                if (0 == n) r.Append("0 ");
                if (n % 1000 != 0)
                    r.Append(RusNumber.Str(n, male, seniorOne, seniorTwo, seniorFive));
                else
                    r.Append(seniorFive);

                n /= 1000;

                r.Insert(0, RusNumber.Str(n, false, "тысяча", "тысячи", "тысяч"));
                n /= 1000;

                r.Insert(0, RusNumber.Str(n, true, "миллион", "миллиона", "миллионов"));
                n /= 1000;

                r.Insert(0, RusNumber.Str(n, true, "миллиард", "миллиарда", "миллиардов"));
                n /= 1000;

                r.Insert(0, RusNumber.Str(n, true, "триллион", "триллиона", "триллионов"));
                n /= 1000;

                r.Insert(0, RusNumber.Str(n, true, "триллиард", "триллиарда", "триллиардов"));
                if (minus) r.Insert(0, "минус ");

                r.Append(remainder.ToString("00 "));
                r.Append(RusNumber.Case(remainder, juniorOne, juniorTwo, juniorFive));

                //Делаем первую букву заглавной
                r[0] = char.ToUpper(r[0]);

                return r.ToString();
            }
        };
        //------------------------------------------------------------------------------------------------------------------------------

        public IActionResult Banks()
        {
            List<Banks> listbanks = new List<Banks>();
            listbanks = db.Banks.Include(i => i.City.Country).OrderBy(f => f.Name).ToList();

            return View(listbanks);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Employee()
        {
            var phone = HttpContext.User.FindAll(ClaimTypes.Role);

            List<Employee> ListEmpl = new List<Employee>();

            foreach (var item in phone)
            {
                // ListEmpl = db.Employee.Include(q => q.Filial).Include(o => o.Department).Include(oo => oo.Position).Where(i=>i.FilialId == Convert.ToInt32(item.Value)).OrderBy(d => d.FirstName).ToList();
                ListEmpl.AddRange(db.Employee.Include(q => q.Filial).Include(o => o.Department).Include(oo => oo.Position).Where(i => i.FilialId == Convert.ToInt32(item.Value)).OrderBy(d => d.FirstName).ToList());
            }

            return View(ListEmpl);
        }

        public IActionResult Komissiya()
        {
            //Это старый вывод комиссии. Какую-то херню написал, сам не понял нахера так сложно сделал
            //var phone = HttpContext.User.FindAll(ClaimTypes.Role);

            //List<Komissiya> ListEmpl = new List<Komissiya>();

            //foreach (var item in phone)
            //{
            //    ListEmpl.AddRange(db.Komissiya.Include(q => q.Employee.Filial).Include(o => o.Employee.Position).Include(oo => oo.Employee.Department).Include(ww => ww.Status).Where(i => i.Employee.FilialId == Convert.ToInt32(item.Value)).OrderBy(j => j.Employee.FilialId).OrderBy(d => d.EmployeeId).ToList());
            //}

            //return View(ListEmpl);
            List<Komissiya> ListEmpl = new List<Komissiya>();

            var usID = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            ListEmpl = db.Komissiya.Include(o => o.Employee.Position).Include(oo => oo.Employee.Department).Include(ww => ww.Status).Where(i => i.Employee.FilialId == UsFil.Employee.FilialId).OrderBy(h => h.Status.Priznak).ToList();

            return View(ListEmpl);

        }

        public IActionResult Child()
        {
            var phone = HttpContext.User.FindAll(ClaimTypes.Role);

            List<Child> ListChild = new List<Child>();

            foreach (var item in phone)
            {
                ListChild.AddRange(db.Child.Include(oa => oa.Employee.Filial).Include(adw => adw.Status).Where(i => i.Employee.Filial.Id == Convert.ToInt32(item.Value)).OrderBy(da => da.Fio).ToList());
            }


            return View(ListChild);
        }

        public IActionResult Dogovor()
        {
            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Dogovor> ListDog = new List<Dogovor>();
            ListDog = db.Dogovor.Include(oa => oa.Sanatorium.City).Include(j => j.TypeDog).Where(i => i.FillialId == UsFil.Employee.FilialId).OrderByDescending(da => da.DateModif).ToList();

            return View(ListDog);
        }



        public IActionResult City()
        {
            List<City> ListCity = new List<City>();
            ListCity = db.City.Include(oa => oa.Country).OrderBy(da => da.Name).ToList();
            return View(ListCity);
        }

        public IActionResult Country()
        {
            List<Country> ListCountry = new List<Country>();
            ListCountry = db.Country.OrderBy(da => da.Name).ToList();
            return View(ListCountry);
        }

        public IActionResult Sanatorium()
        {
            List<Sanatorium> ListSanatorium = new List<Sanatorium>();
            ListSanatorium = db.Sanatorium.Include(y => y.City.Country).OrderBy(da => da.Name).ToList();
            return View(ListSanatorium);
        }

        public IActionResult Filial()
        {
            List<Fillial> ListFillial = new List<Fillial>();
            ListFillial = db.Fillial.OrderBy(da => da.Name).ToList();
            return View(ListFillial);
        }

        public IActionResult Department()
        {
            List<Department> ListDepartment = new List<Department>();
            ListDepartment = db.Department.OrderBy(da => da.Name).ToList();
            return View(ListDepartment);
        }

        public IActionResult Position()
        {
            List<Position> ListPosition = new List<Position>();
            ListPosition = db.Position.OrderBy(da => da.Name).ToList();
            return View(ListPosition);
        }

        public IActionResult Protocol()
        {
            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            DateTime today = DateTime.Today;
            DateTime firstOfMonth = new DateTime(today.Year, 01, 01);
            DateTime firstlastmonth = firstOfMonth.AddMonths(1);

            List<Protocol> ListProtocol = new List<Protocol>();
            ListProtocol = db.Protocol.Include(y => y.Filial).Where(i => i.Filial.Id == UsFil.Employee.FilialId && i.DateProt >= firstOfMonth && i.DateProt <= DateTime.Now).OrderByDescending(da => da.DateProt).ToList();
            return View(ListProtocol);
        }
        public IActionResult Zayavlenie()
        {
            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            DateTime today = DateTime.Today;
            //DateTime firstOfMonth = new DateTime(today.Year, today.Month, 01);
            DateTime firstOfMonth = new DateTime();
            firstOfMonth = DateTime.Now.AddMonths(-2);
            DateTime firstlastmonth = firstOfMonth.AddMonths(1);

            List<Zayavlenie> ListZayavlenie = new List<Zayavlenie>();
            ListZayavlenie = db.Zayavlenie.Include(y => y.TableZay).Include(h => h.Protocol).Include(g => g.Employee).Include(p => p.Sanatorium).Where(i => i.Protocol.Filial.Id == UsFil.Employee.FilialId && i.DateZ >= firstOfMonth && i.DateZ <= DateTime.Now).OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            return View(ListZayavlenie);
        }

        public IActionResult LoginPassword()
        {
            List<User> ListUser = new List<User>();
            ListUser = db.User.Include(f => f.Employee.Filial).OrderBy(s => s.Name).ToList();
            return View(ListUser);
        }

        //----------Добавление пользователя-----------------------//

        public ActionResult AddLoginPassword()
        {
            List<Employee> ListEmpl = new List<Employee>();
            ListEmpl = db.Employee.OrderBy(f => f.FirstName).ToList();
            ViewBag.Employee = ListEmpl;

            List<Roles> rol = new List<Roles>();
            rol = db.Roles.ToList();
            ViewBag.Rol = rol;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления пользователя-----------//
        [HttpPost]
        public ActionResult LoginPasswordSave([FromBody] LoginPassword mod)
        {

            try
            {
                User us = new User();
                us.Name = mod.Login;
                us.Password = md5.hashPassword(mod.Password);
                us.EmployeeId = mod.EmployeeID;
                us.UserModif = mod.UserModific;
                us.DateMod = DateTime.Now;
                db.User.Add(us);

                db.SaveChanges();
                //Находим ID созданного пользователя и получаем его для записи в таблицу UserRoles
                List<User> usList = new List<User>();
                usList = db.User.OrderByDescending(h => h.Id).ToList();

                User UsLast = new User();
                UsLast = usList.FirstOrDefault();// это только что созданный пользователь

                //Теперь заполним таблицу с ролями для этого пользователя

                foreach (var item in mod.Rol)
                {
                    UserRole UR = new UserRole();
                    UR.UserId = UsLast.Id;
                    UR.RoleId = Convert.ToInt32(item);
                    db.UserRole.Add(UR);
                    db.SaveChanges();
                }


                ViewBag.Message = "Пользователь успешно добавлен!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление пользователя//

        public ActionResult DeleteUser(int ID)
        {
            User userDel = new User();
            userDel = db.User.FirstOrDefault(a => a.Id == ID);
            return PartialView(userDel);
        }
        //-----------------------------//

        // Подтверждение удаления пользователя//
        public ActionResult DeleteUserOK(int ID)
        {
            try
            {
                List<UserRole> UsRol = new List<UserRole>();
                UsRol = db.UserRole.Where(h => h.UserId == ID).ToList();

                db.UserRole.RemoveRange(UsRol);
                db.SaveChanges();

                User userDS = new User();
                userDS = db.User.FirstOrDefault(a => a.Id == ID);
                db.User.Remove(userDS);
                db.SaveChanges();

                ViewBag.Message = "Пользователь удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование пользователя//

        public ActionResult UserEdit(int ID)
        {
            User UserEd = new User();
            UserEd = db.User.FirstOrDefault(a => a.Id == ID);

            List<Employee> ListEmpl = new List<Employee>();
            ListEmpl = db.Employee.OrderBy(f => f.FirstName).ToList();
            ViewBag.Employee = ListEmpl;

            List<UserRole> ur = new List<UserRole>();
            ur = db.UserRole.Where(j => j.UserId == ID).ToList();
            ViewBag.UR = ur;

            List<Roles> LR = new List<Roles>();
            LR = db.Roles.ToList();
            ViewBag.LR = LR;



            return PartialView(UserEd);
        }
        //-------------------------------//

        //Сохранение редактирования пользователя------------//
        [HttpPost]
        public ActionResult UserEditSave([FromBody] LoginPassword modus)
        {
            try
            {
                User userE = new User();
                userE = db.User.FirstOrDefault(s => s.Id == modus.Id);

                userE.Name = modus.Login.Trim();
                userE.Password = md5.hashPassword(modus.Password);
                userE.EmployeeId = modus.EmployeeID;
                userE.UserModif = modus.UserModific;
                userE.DateMod = DateTime.Now;
                db.SaveChanges();

                //Работаем с таблицей UserRole                              

                List<UserRole> UsRol = new List<UserRole>();
                UsRol = db.UserRole.Where(h => h.UserId == modus.Id).ToList();

                db.UserRole.RemoveRange(UsRol);
                db.SaveChanges();


                foreach (var it in modus.Rol)
                {
                    UserRole ur = new UserRole();
                    ur.UserId = modus.Id;
                    ur.RoleId = Convert.ToInt32(it);
                    db.UserRole.Add(ur);
                    db.SaveChanges();
                }


                ViewBag.Message = "Пользователь изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//

        public ActionResult Reference()
        {

            return PartialView();
        }
        //--------------------ДОЛЖНОСТИ------------------------------------------------------------------
        //----------Добавление должности-----------------------//

        public ActionResult AddPosition()
        {
            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления должности-----------//
        [HttpPost]
        public ActionResult PositionSave([FromBody] LoginPassword mod)
        {

            try
            {
                Position us = new Position();
                us.Name = mod.Login;
                us.Priznak = mod.Password;
                us.UserMod = mod.UserModific;
                us.DateMod = DateTime.Now;
                db.Position.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Должность успешно добавлена!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление должности//

        public ActionResult DeletePosition(int ID)
        {
            Position positionDel = new Position();
            positionDel = db.Position.FirstOrDefault(a => a.Id == ID);
            return PartialView(positionDel);
        }
        //-----------------------------//

        // Подтверждение удаления должности//
        public ActionResult DeletePositionOK(int ID)
        {
            try
            {
                Position positionDS = new Position();
                positionDS = db.Position.FirstOrDefault(a => a.Id == ID);
                db.Position.Remove(positionDS);
                db.SaveChanges();

                ViewBag.Message = "Должность удалена!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование должности//

        public ActionResult PositionEdit(int ID)
        {
            Position UserEd = new Position();
            UserEd = db.Position.FirstOrDefault(a => a.Id == ID);

            return PartialView(UserEd);
        }
        //-------------------------------//

        //Сохранение редактирования должности------------//
        [HttpPost]
        public ActionResult PositionEditSave([FromBody] LoginPassword modus)
        {
            try
            {
                Position userE = new Position();
                userE = db.Position.FirstOrDefault(s => s.Id == modus.Id);

                userE.Name = modus.Login.Trim();
                userE.Priznak = modus.Password;
                userE.UserMod = modus.UserModific;
                userE.DateMod = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "Должность изменена";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//
        //-----------------------------------------------------------------------------------------------

        //--------------------ОТДЕЛЫ------------------------------------------------------------------
        //----------Добавление отдела-----------------------//

        public ActionResult AddDepartment()
        {
            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления отдела-----------//
        [HttpPost]
        public ActionResult DepartmentSave([FromBody] LoginPassword mod)
        {

            try
            {
                Department us = new Department();
                us.Name = mod.Login;
                us.Priznak = mod.Password;
                us.UserMod = mod.UserModific;
                us.DateMod = DateTime.Now;
                db.Department.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Отдел успешно добавлен!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление отдела//

        public ActionResult DeleteDepartment(int ID)
        {
            Department departmentDel = new Department();
            departmentDel = db.Department.FirstOrDefault(a => a.Id == ID);
            return PartialView(departmentDel);
        }
        //-----------------------------//

        // Подтверждение удаления отдела//
        public ActionResult DeleteDepartmentOK(int ID)
        {
            try
            {
                Department departmentDS = new Department();
                departmentDS = db.Department.FirstOrDefault(a => a.Id == ID);
                db.Department.Remove(departmentDS);
                db.SaveChanges();

                ViewBag.Message = "Отдел удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование отдела//

        public ActionResult DepartmentEdit(int ID)
        {
            Department UserEd = new Department();
            UserEd = db.Department.FirstOrDefault(a => a.Id == ID);

            return PartialView(UserEd);
        }
        //-------------------------------//

        //Сохранение редактирования отдела------------//
        [HttpPost]
        public ActionResult DepartmentEditSave([FromBody] LoginPassword modus)
        {
            try
            {
                Department userE = new Department();
                userE = db.Department.FirstOrDefault(s => s.Id == modus.Id);

                userE.Name = modus.Login.Trim();
                userE.Priznak = modus.Password;
                userE.UserMod = modus.UserModific;
                userE.DateMod = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "Отдел изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//
        //--------------------ПОДРАЗДЕЛЕНИЯ------------------------------------------------------------------
        //----------Добавление подразделения-----------------------//

        public ActionResult AddFilial()
        {
            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления подразделения-----------//
        [HttpPost]
        public ActionResult FilialSave([FromBody] LoginPassword mod)
        {

            try
            {
                Fillial us = new Fillial();
                us.Name = mod.Login;
                us.Prefix = mod.Password;
                us.UserMod = mod.UserModific;
                us.DateMod = DateTime.Now;
                db.Fillial.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Подразделение успешно добавлено!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление подразделения//

        public ActionResult DeleteFilial(int ID)
        {
            Fillial filialDel = new Fillial();
            filialDel = db.Fillial.FirstOrDefault(a => a.Id == ID);
            return PartialView(filialDel);
        }
        //-----------------------------//

        // Подтверждение удаления подразделения//
        public ActionResult DeleteFilialOK(int ID)
        {
            try
            {
                Fillial filialDS = new Fillial();
                filialDS = db.Fillial.FirstOrDefault(a => a.Id == ID);
                db.Fillial.Remove(filialDS);
                db.SaveChanges();

                ViewBag.Message = "Подразделение удалено!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование подразделения//

        public ActionResult FilialEdit(int ID)
        {
            Fillial UserEd = new Fillial();
            UserEd = db.Fillial.FirstOrDefault(a => a.Id == ID);

            return PartialView(UserEd);
        }
        //-------------------------------//

        //Сохранение редактирования подразделения------------//
        [HttpPost]
        public ActionResult FilialEditSave([FromBody] LoginPassword modus)
        {
            try
            {
                Fillial userE = new Fillial();
                userE = db.Fillial.FirstOrDefault(s => s.Id == modus.Id);

                userE.Name = modus.Login.Trim();
                userE.Prefix = modus.Password;
                userE.UserMod = modus.UserModific;
                userE.DateMod = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "Подразделение изменено";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//
        //--------------------ГОРОДА------------------------------------------------------------------
        //----------Добавление города-----------------------//

        public ActionResult AddCity()
        {
            List<Country> listcountry = new List<Country>();
            listcountry = db.Country.OrderBy(h => h.Name).ToList();
            ViewBag.listcountry = listcountry;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления города-----------//
        [HttpPost]
        public ActionResult CitySave([FromBody] LoginPassword mod)
        {

            try
            {
                City us = new City();
                us.Name = mod.Login;
                us.CountryId = mod.EmployeeID;
                us.UserMod = mod.UserModific;
                us.DateMod = DateTime.Now;
                db.City.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Город успешно добавлен!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление города//

        public ActionResult DeleteCity(int ID)
        {
            City filialDel = new City();
            filialDel = db.City.FirstOrDefault(a => a.Id == ID);
            return PartialView(filialDel);
        }
        //-----------------------------//

        // Подтверждение удаления города//
        public ActionResult DeleteCityOK(int ID)
        {
            try
            {
                City cityDS = new City();
                cityDS = db.City.FirstOrDefault(a => a.Id == ID);
                db.City.Remove(cityDS);
                db.SaveChanges();

                ViewBag.Message = "город удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование города//

        public ActionResult cityEdit(int ID)
        {
            List<Country> listcountry = new List<Country>();
            listcountry = db.Country.OrderBy(h => h.Name).ToList();
            ViewBag.listcountry = listcountry;

            City UserEd = new City();
            UserEd = db.City.FirstOrDefault(a => a.Id == ID);

            return PartialView(UserEd);
        }
        //-------------------------------//

        //Сохранение редактирования города------------//
        [HttpPost]
        public ActionResult CityEditSave([FromBody] LoginPassword modus)
        {
            try
            {
                City userE = new City();
                userE = db.City.FirstOrDefault(s => s.Id == modus.Id);

                userE.Name = modus.Login.Trim();
                userE.CountryId = modus.EmployeeID;
                userE.UserMod = modus.UserModific;
                userE.DateMod = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "город изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//

        //--------------------Страны    ------------------------------------------------------------------
        //----------Добавление страны-----------------------//

        public ActionResult AddCountry()
        {
            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления города-----------//
        [HttpPost]
        public ActionResult CountrySave([FromBody] LoginPassword mod)
        {

            try
            {
                Country us = new Country();
                us.Name = mod.Login;
                us.UserMod = mod.UserModific;
                us.DateMod = DateTime.Now;
                db.Country.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Страна успешно добавлена!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление страны//

        public ActionResult DeleteCountry(int ID)
        {
            Country countryDel = new Country();
            countryDel = db.Country.FirstOrDefault(a => a.Id == ID);
            return PartialView(countryDel);
        }
        //-----------------------------//

        // Подтверждение удаления страны//
        public ActionResult DeleteCountryOK(int ID)
        {
            try
            {
                Country countryDS = new Country();
                countryDS = db.Country.FirstOrDefault(a => a.Id == ID);
                db.Country.Remove(countryDS);
                db.SaveChanges();

                ViewBag.Message = "страна удалена!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование страны//

        public ActionResult CountryEdit(int ID)
        {
            Country countryEd = new Country();
            countryEd = db.Country.FirstOrDefault(a => a.Id == ID);

            return PartialView(countryEd);
        }
        //-------------------------------//

        //Сохранение редактирования страны------------//
        [HttpPost]
        public ActionResult CountryEditSave([FromBody] LoginPassword modus)
        {
            try
            {
                Country userE = new Country();
                userE = db.Country.FirstOrDefault(s => s.Id == modus.Id);

                userE.Name = modus.Login.Trim();
                userE.UserMod = modus.UserModific;
                userE.DateMod = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "страна изменена";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//
        //--------------------Банки------------------------------------------------------------------
        //----------Добавление банка-----------------------//

        public ActionResult AddBank()
        {
            List<City> listcity = new List<City>();
            listcity = db.City.OrderBy(h => h.Name).ToList();
            ViewBag.listcity = listcity;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления банка-----------//
        [HttpPost]
        public ActionResult BankSave([FromBody] Banks mod)
        {

            try
            {
                Banks us = new Banks();
                us.Name = mod.Name;
                us.CityId = mod.CityId;
                us.Address = mod.Address;
                us.Unp = mod.Unp;
                us.Okpo = mod.Okpo;
                us.Bic = mod.Bic;
                us.UserModif = mod.UserModif;
                us.DateModif = DateTime.Now;
                db.Banks.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Банк успешно добавлен!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление банка//

        public ActionResult DeleteBank(int ID)
        {
            Banks bankDel = new Banks();
            bankDel = db.Banks.FirstOrDefault(a => a.Id == ID);
            return PartialView(bankDel);
        }
        //-----------------------------//

        // Подтверждение удаления банка//
        public ActionResult DeleteBankOK(int ID)
        {
            try
            {
                Banks bankDS = new Banks();
                bankDS = db.Banks.FirstOrDefault(a => a.Id == ID);
                db.Banks.Remove(bankDS);
                db.SaveChanges();

                ViewBag.Message = "банк удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование банка//

        public ActionResult BankEdit(int ID)
        {
            List<City> listcity = new List<City>();
            listcity = db.City.OrderBy(h => h.Name).ToList();
            ViewBag.listcity = listcity;

            Banks ban = new Banks();
            ban = db.Banks.FirstOrDefault(a => a.Id == ID);

            return PartialView(ban);
        }
        //-------------------------------//

        //Сохранение редактирования банка------------//
        [HttpPost]
        public ActionResult BankEditSave([FromBody] Banks mod)
        {
            try
            {
                Banks bankE = new Banks();
                bankE = db.Banks.FirstOrDefault(s => s.Id == mod.Id);

                bankE.Name = mod.Name;
                bankE.CityId = mod.CityId;
                bankE.Rs = mod.Rs;
                bankE.Address = mod.Address;
                bankE.Unp = mod.Unp;
                bankE.Okpo = mod.Okpo;
                bankE.Bic = mod.Bic;
                bankE.UserModif = mod.UserModif;
                bankE.DateModif = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "банк изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//

        //--------------------САНАТОРИИ------------------------------------------------------------------
        //----------Добавление санатория-----------------------//

        public ActionResult AddSanatorium()
        {
            List<City> listcountry = new List<City>();
            listcountry = db.City.OrderBy(y => y.Name).ToList();
            ViewBag.listcountry = listcountry;

            List<Banks> listbank = new List<Banks>();
            listbank = db.Banks.OrderBy(y => y.Name).ToList();
            ViewBag.listbank = listbank;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления страны-----------//
        [HttpPost]
        public ActionResult SanatoriumSave([FromBody] Sanatorium mod)
        {

            try
            {
                Sanatorium us = new Sanatorium();
                us.Name = mod.Name;
                us.CityId = mod.CityId;
                us.Address = mod.Address;
                us.PostAddress = mod.PostAddress;
                us.Unp = mod.Unp;
                us.BankId = mod.BankId;
                us.SanatInd = mod.SanatInd;
                us.Priznak = mod.Priznak;
                us.UserMod = mod.UserMod;
                us.DateMod = DateTime.Now;
                db.Sanatorium.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Санторий успешно добавлен!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление санатория//

        public ActionResult DeleteSanatorium(int ID)
        {
            Sanatorium filialDel = new Sanatorium();
            filialDel = db.Sanatorium.FirstOrDefault(a => a.Id == ID);
            return PartialView(filialDel);
        }
        //-----------------------------//

        // Подтверждение удаления санатория//
        public ActionResult DeleteSanatoriumOK(int ID)
        {
            try
            {
                Sanatorium sanatoriumDS = new Sanatorium();
                sanatoriumDS = db.Sanatorium.FirstOrDefault(a => a.Id == ID);
                db.Sanatorium.Remove(sanatoriumDS);
                db.SaveChanges();

                ViewBag.Message = "санаторий удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование санатория//

        public ActionResult SanatoriumEdit(int ID)
        {
            List<City> listcountry = new List<City>();
            listcountry = db.City.OrderBy(y => y.Name).ToList();
            ViewBag.listcountry = listcountry;

            List<Banks> listbank = new List<Banks>();
            listbank = db.Banks.OrderBy(y => y.Name).ToList();
            ViewBag.listbank = listbank;

            Sanatorium UserEd = new Sanatorium();
            UserEd = db.Sanatorium.FirstOrDefault(a => a.Id == ID);

            return PartialView(UserEd);
        }
        //-------------------------------//

        //Сохранение редактирования санатория------------//
        [HttpPost]
        public ActionResult SanatoriumEditSave([FromBody] Sanatorium mod)
        {
            try
            {
                Sanatorium sanatoriumE = new Sanatorium();
                sanatoriumE = db.Sanatorium.FirstOrDefault(s => s.Id == mod.Id);

                sanatoriumE.Name = mod.Name;
                sanatoriumE.CityId = mod.CityId;
                sanatoriumE.Address = mod.Address;
                sanatoriumE.PostAddress = mod.PostAddress;
                sanatoriumE.BankId = mod.BankId;
                sanatoriumE.Unp = mod.Unp;
                sanatoriumE.Priznak = mod.Priznak;
                sanatoriumE.SanatInd = mod.SanatInd;
                sanatoriumE.UserMod = mod.UserMod;
                sanatoriumE.DateMod = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "санаторий изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//

        //--------------------СОТРУДНИКИ------------------------------------------------------------------
        //----------Добавление сотрудника-----------------------//

        public ActionResult AddEmployee()
        {
            List<Position> listposition = new List<Position>();
            listposition = db.Position.OrderBy(u => u.Name).ToList();
            ViewBag.listposition = listposition;

            List<Department> listdepartment = new List<Department>();
            listdepartment = db.Department.OrderBy(uu => uu.Name).ToList();
            ViewBag.listdepartment = listdepartment;

            List<Fillial> listfillial = new List<Fillial>();
            listfillial = db.Fillial.OrderBy(uuu => uuu.Name).ToList();
            ViewBag.listfillial = listfillial;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления сотрудника-----------//
        [HttpPost]
        public ActionResult EmployeeSave([FromBody] Employee mod)
        {

            try
            {
                Employee us = new Employee();
                us.FirstName = mod.FirstName;
                us.LastName = mod.LastName;
                us.MiddleName = mod.MiddleName;
                us.PositionId = mod.PositionId;
                us.DepartmentId = mod.DepartmentId;
                us.FilialId = mod.FilialId;
                us.Address = mod.Address;
                us.DateBirth = mod.DateBirth;
                us.Pol = mod.Pol;
                us.UserMod = mod.UserMod;
                us.DateMod = DateTime.Now;
                db.Employee.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Сотрудник успешно добавлен!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление сотрудника//

        public ActionResult DeleteEmployee(int ID)
        {
            Employee filialDel = new Employee();
            filialDel = db.Employee.FirstOrDefault(a => a.Id == ID);
            return PartialView(filialDel);
        }
        //-----------------------------//

        // Подтверждение удаления сотрудника//
        public ActionResult DeleteEmployeeOK(int ID)
        {
            try
            {
                Employee cityDS = new Employee();
                cityDS = db.Employee.FirstOrDefault(a => a.Id == ID);
                db.Employee.Remove(cityDS);
                db.SaveChanges();

                ViewBag.Message = "сотрудник удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование сотрудника//

        public ActionResult EmployeeEdit(int ID)
        {
            List<Position> listposition = new List<Position>();
            listposition = db.Position.OrderBy(u => u.Name).ToList();
            ViewBag.listposition = listposition;

            List<Department> listdepartment = new List<Department>();
            listdepartment = db.Department.OrderBy(uu => uu.Name).ToList();
            ViewBag.listdepartment = listdepartment;

            List<Fillial> listfillial = new List<Fillial>();
            listfillial = db.Fillial.OrderBy(uuu => uuu.Name).ToList();
            ViewBag.listfillial = listfillial;

            Employee UserEd = new Employee();
            UserEd = db.Employee.FirstOrDefault(a => a.Id == ID);

            return PartialView(UserEd);
        }
        //-------------------------------//

        //Сохранение редактирования сотрудника------------//
        [HttpPost]
        public ActionResult EmployeeEditSave([FromBody] Employee modus)
        {
            try
            {
                Employee userE = new Employee();
                userE = db.Employee.FirstOrDefault(s => s.Id == modus.Id);

                userE.FirstName = modus.FirstName;
                userE.LastName = modus.LastName;
                userE.MiddleName = modus.MiddleName;
                userE.PositionId = modus.PositionId;
                userE.DepartmentId = modus.DepartmentId;
                userE.FilialId = modus.FilialId;
                userE.Address = modus.Address;
                userE.Pol = modus.Pol;
                userE.DateBirth = modus.DateBirth;
                userE.UserMod = modus.UserMod;
                userE.DateMod = DateTime.Now;
                userE.Fiorp = modus.Fiorp;
                userE.Fiodp = modus.Fiodp;
                userE.Fiovp = modus.Fiovp;
                userE.Fiotp = modus.Fiotp;
                userE.Fiopp = modus.Fiopp;

                db.SaveChanges();

                ViewBag.Message = "сотрудник изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//

        //--------------------Родственники------------------------------------------------------------------
        //----------Добавление родственника-----------------------//

        public ActionResult AddChild()
        {
            List<Employee> listemployee = new List<Employee>();
            listemployee = db.Employee.OrderBy(u => u.FirstName).ToList();
            ViewBag.listemployee = listemployee;

            List<Status> liststatus = new List<Status>();
            liststatus = db.Status.ToList();
            ViewBag.liststatus = liststatus;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления родственника-----------//
        [HttpPost]
        public ActionResult ChildSave([FromBody] Child mod)
        {

            try
            {
                Child us = new Child();
                us.Fio = mod.Fio;
                us.EmployeeId = mod.EmployeeId;
                us.StatusId = mod.StatusId;
                us.Pol = mod.Pol;
                us.DateBirth = mod.DateBirth;
                us.UserMod = mod.UserMod;
                us.DateMod = DateTime.Now;
                db.Child.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Родственник успешно добавлен!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление родственника//

        public ActionResult DeleteChild(int ID)
        {
            Child childDel = new Child();
            childDel = db.Child.FirstOrDefault(a => a.Id == ID);
            return PartialView(childDel);
        }
        //-----------------------------//

        // Подтверждение удаления родственника//
        public ActionResult DeleteChildOK(int ID)
        {
            try
            {
                Child cityDS = new Child();
                cityDS = db.Child.FirstOrDefault(a => a.Id == ID);
                db.Child.Remove(cityDS);
                db.SaveChanges();

                ViewBag.Message = "родственник удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование родственника//

        public ActionResult ChildEdit(int ID)
        {
            List<Employee> listemployee = new List<Employee>();
            listemployee = db.Employee.OrderBy(u => u.FirstName).ToList();
            ViewBag.listemployee = listemployee;

            List<Status> liststatus = new List<Status>();
            liststatus = db.Status.ToList();
            ViewBag.liststatus = liststatus;

            Child UserEd = new Child();
            UserEd = db.Child.FirstOrDefault(a => a.Id == ID);

            return PartialView(UserEd);
        }
        //-------------------------------//

        //Сохранение редактирования родственника------------//
        [HttpPost]
        public ActionResult ChildEditSave([FromBody] Child modus)
        {
            try
            {
                Child userE = new Child();
                userE = db.Child.FirstOrDefault(s => s.Id == modus.Id);

                userE.Fio = modus.Fio;
                userE.EmployeeId = modus.EmployeeId;
                userE.StatusId = modus.StatusId;
                userE.Pol = modus.Pol;
                userE.DateBirth = modus.DateBirth;
                userE.UserMod = modus.UserMod;
                userE.DateMod = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "Родственник изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//

        //--------------------ПРОТОКОЛЫ------------------------------------------------------------------
        //----------Добавление протокола-----------------------//

        public ActionResult AddProtocol()
        {
            List<Fillial> listfilial = new List<Fillial>();
            listfilial = db.Fillial.OrderBy(y => y.Name).ToList();
            ViewBag.listfilial = listfilial;

            var usID = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<Protocol> LP = new List<Protocol>();
            LP = db.Protocol.Where(i => i.Filial.Id == UsFil.Employee.Filial.Id).OrderByDescending(u => u.DateProt).OrderByDescending(h => h.NumberP).ToList();

            //Получаем последнюю запись в списке протоколов
            Protocol PrEnd = new Protocol();
            PrEnd = LP.FirstOrDefault();

            //Получаем последний номер протокола
            int? numberProt = 0;
            if (LP.Count == 0)
            {
                numberProt = 1;
            }
            else if (DateTime.Now.Year == Convert.ToDateTime(PrEnd.DateProt).Year)
            {
                numberProt = PrEnd.NumberP + 1;
            }
            else if (DateTime.Now.Year == Convert.ToDateTime(PrEnd.DateProt).Year)
            {
                numberProt = 1;
            }

            ViewBag.numberProt = numberProt;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления протокола-----------//
        [HttpPost]
        public ActionResult ProtocolSave([FromBody] LoginPassword mod)
        {

            try
            {
                User u = new User();
                u = db.User.FirstOrDefault(g => g.Name == mod.UserModific);

                //Находим ID филиала пользователя
                Employee EMPL = new Employee();
                EMPL = db.Employee.FirstOrDefault(h => h.Id == u.EmployeeId);

                Protocol us = new Protocol();
                us.NumberP = Convert.ToInt32(mod.Login);
                us.DateProt = mod.DateMod;
                us.FilialId = EMPL.FilialId;
                us.UserModific = mod.UserModific;
                us.DateModific = DateTime.Now;
                db.Protocol.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Протокол успешно добавлен!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление протокола//

        public ActionResult DeleteProtocol(int ID)
        {
            Protocol filialDel = new Protocol();
            filialDel = db.Protocol.FirstOrDefault(a => a.Id == ID);
            return PartialView(filialDel);
        }
        //-----------------------------//

        // Подтверждение удаления протокола//
        public ActionResult DeleteProtocolOK(int ID)
        {
            try
            {
                Protocol protDS = new Protocol();
                protDS = db.Protocol.FirstOrDefault(a => a.Id == ID);
                db.Protocol.Remove(protDS);
                db.SaveChanges();

                ViewBag.Message = "Протокол удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование протокола//

        public ActionResult ProtocolEdit(int ID)
        {
            List<Fillial> listfilial = new List<Fillial>();
            listfilial = db.Fillial.OrderBy(y => y.Name).ToList();
            ViewBag.listfilial = listfilial;

            Protocol UserEd = new Protocol();
            UserEd = db.Protocol.FirstOrDefault(a => a.Id == ID);

            return PartialView(UserEd);
        }
        //-------------------------------//

        //Сохранение редактирования протокола------------//
        [HttpPost]
        public ActionResult ProtocolEditSave([FromBody] LoginPassword modus)
        {
            try
            {
                User u = new User();
                u = db.User.FirstOrDefault(g => g.Name == modus.UserModific);

                //Находим ID филиала пользователя
                Employee EMPL = new Employee();
                EMPL = db.Employee.FirstOrDefault(h => h.Id == u.EmployeeId);

                Protocol protocolE = new Protocol();
                protocolE = db.Protocol.FirstOrDefault(s => s.Id == modus.Id);

                protocolE.NumberP = Convert.ToInt32(modus.Login.Trim());
                protocolE.DateProt = modus.DateMod;
                protocolE.FilialId = EMPL.FilialId;
                protocolE.UserModific = modus.UserModific;
                protocolE.DateModific = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "Протокол изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//
        //--------------------ЗАЯВЛЕНИЯ------------------------------------------------------------------
        //----------Добавление заявления-----------------------//

        public ActionResult AddZay()
        {
            var us = HttpContext.User.Identity?.Name;
            User userLog = new User();
            userLog = db.User.Include(g => g.Employee.Filial).FirstOrDefault(h => h.Name == us);

            List<Protocol> listprot = new List<Protocol>();
            listprot = db.Protocol.OrderByDescending(y => y.DateProt).Where(i => i.FilialId == userLog.Employee.FilialId).ToList();
            ViewBag.listprot = listprot;

            List<Employee> listempl = new List<Employee>();
            listempl = db.Employee.OrderBy(y => y.FirstName).Where(i => i.FilialId == userLog.Employee.FilialId).ToList();
            ViewBag.listempl = listempl;

            List<Sanatorium> listsanat = new List<Sanatorium>();
            listsanat = db.Sanatorium.OrderBy(y => y.Name).ToList();
            ViewBag.listsanat = listsanat;

            //Получаем список договоров данного филлиала для списка санаториев с которыми заключен договор
            List<Dogovor> ListDog = new List<Dogovor>();
            ListDog = db.Dogovor.Include(g => g.Sanatorium).Where(i => i.FillialId == userLog.Employee.FilialId).ToList();
            var ld = ListDog.GroupBy(h => h.Sanatorium).Select(grt => new { Name = grt.Key.Name, id = grt.Key.Id, spis = grt.Select(p => p) }).OrderBy(h => h.Name);
            ViewBag.listDog = ld;

            //Получим номер заявления по порядку в текущем году

            var usID = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<Zayavlenie> LZ = new List<Zayavlenie>();
            LZ = db.Zayavlenie.Where(i => i.Protocol.FilialId == UsFil.Employee.Filial.Id).OrderByDescending(u => u.DateZ).OrderByDescending(h => h.NumberZ).ToList();

            //Получаем последнюю запись в списке протоколов
            Zayavlenie ZEnd = new Zayavlenie();
            ZEnd = LZ.FirstOrDefault();

            //Получаем последний номер протокола
            int? numberZay = 0;
            if (LZ.Count == 0)
            {
                numberZay = 1;
            }
            else if (DateTime.Now.Year == Convert.ToDateTime(ZEnd.DateZ).Year)
            {
                numberZay = ZEnd.NumberZ + 1;
            }
            else if (DateTime.Now.Year == Convert.ToDateTime(ZEnd.DateZ).Year)
            {
                numberZay = 1;
            }

            ViewBag.numberZay = numberZay;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления заявления-----------//
        [HttpPost]
        public ActionResult ZaySave([FromBody] ZayTab mod)
        {

            try
            {
                User u = new User();
                u = db.User.FirstOrDefault(g => g.Name == mod.UserMod);

                //Находим ID филиала пользователя
                Employee EMPL = new Employee();
                EMPL = db.Employee.FirstOrDefault(h => h.Id == u.EmployeeId);

                Zayavlenie za = new Zayavlenie();
                za.NumberZ = Convert.ToInt32(mod.NumberZ);
                za.DateZ = mod.DateZ;
                za.ProtocolId = mod.ProtocolId;
                za.EmployeeId = mod.EmployeeId;
                za.SanatoriumId = mod.SanatoriumId;
                za.S = mod.S;
                za.Po = mod.Po;
                za.Who = mod.Who;
                za.PriznakOplata = 0;
                za.Anulirovano = 0;
                za.Priznak = "не принято";
                za.Summa = mod.Summa;
                za.SummaDop = mod.SummaDop;
                za.UserMod = mod.UserMod;
                za.DateMod = DateTime.Now;
                db.Zayavlenie.Add(za);
                db.SaveChanges();

                //Работаем с таблицей TableZay
                if ((mod.Who.Trim() == "детей") || (mod.Who.Trim() == "семейная"))
                {
                    //Находим ID последней записи (только что добавленную запись) в заявлениях
                    List<Zayavlenie> listz1 = new List<Zayavlenie>();
                    listz1 = db.Zayavlenie.OrderByDescending(g => g.DateMod).ToList();
                    int ZAYID1 = listz1.FirstOrDefault().Id;

                    foreach (var item in mod.TableZ)
                    {
                        TableZay ZSD = new TableZay();
                        ZSD.ZayId = ZAYID1;
                        ZSD.ChildId = item;

                        db.TableZay.Add(ZSD);
                        db.SaveChanges();
                    }

                }

                //Работаем с таблицей SUMDOGZAY

                //Получаем список договоров данного филлиала для списка санаториев с которыми заключен договор
                List<Dogovor> ListDog = new List<Dogovor>();
                ListDog = db.Dogovor.Where(i => i.FillialId == EMPL.FilialId && i.SanatoriumId == mod.SanatoriumId).ToList();

                //Находим ID последней записи (только что добавленную запись) в заявлениях
                List<Zayavlenie> listz = new List<Zayavlenie>();
                listz = db.Zayavlenie.OrderByDescending(g => g.DateMod).ToList();
                int ZAYID = listz.FirstOrDefault().Id;


                foreach (var item1 in ListDog)
                {
                    SummDogZay SDZ = new SummDogZay();
                    SDZ.ZayavlenieId = ZAYID;
                    // SDZ.SanatoriumId = Convert.ToInt32(mod.SanatoriumId);
                    SDZ.DogovorId = item1.Id;
                    if (item1.TypeDogId == 1)
                    {
                        SDZ.Summa = mod.Summa;
                    }
                    if (item1.TypeDogId == 2)
                    {
                        SDZ.Summa = mod.SummaDop;
                    }
                    if (item1.TypeDogId == 3)
                    {
                        SDZ.Summa = mod.Summa + mod.SummaDop;
                    }

                    db.SummDogZay.Add(SDZ);
                    db.SaveChanges();
                }


                ViewBag.Message = "Заявление успешно сформировано!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление заявления//

        public ActionResult DeleteZay(int ID)
        {
            Zayavlenie zayDel = new Zayavlenie();
            zayDel = db.Zayavlenie.FirstOrDefault(a => a.Id == ID);
            return PartialView(zayDel);
        }
        //-----------------------------//

        // Подтверждение удаления заявления//
        public ActionResult DeleteZayOK(int ID)
        {
            try
            {
                List<TableZay> TZ = new List<TableZay>();
                TZ = db.TableZay.Where(h => h.ZayId == ID).ToList();

                db.TableZay.RemoveRange(TZ);
                db.SaveChanges();

                List<SummDogZay> SD = new List<SummDogZay>();
                SD = db.SummDogZay.Where(h => h.ZayavlenieId == ID).ToList();

                db.SummDogZay.RemoveRange(SD);
                db.SaveChanges();

                Zayavlenie ZayDS = new Zayavlenie();
                ZayDS = db.Zayavlenie.FirstOrDefault(a => a.Id == ID);
                db.Zayavlenie.Remove(ZayDS);
                db.SaveChanges();

                ViewBag.Message = "Заявление удалено!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование заявления//

        public ActionResult ZayEdit(int ID)
        {
            var us = HttpContext.User.Identity?.Name;
            User userLog = new User();
            userLog = db.User.Include(g => g.Employee.Filial).FirstOrDefault(h => h.Name == us);

            List<Protocol> listprot = new List<Protocol>();
            listprot = db.Protocol.OrderByDescending(y => y.DateProt).Where(i => i.FilialId == userLog.Employee.FilialId).ToList();
            ViewBag.listprot = listprot;

            List<Employee> listempl = new List<Employee>();
            listempl = db.Employee.OrderBy(y => y.FirstName).Where(i => i.FilialId == userLog.Employee.FilialId).ToList();
            ViewBag.listempl = listempl;

            List<Sanatorium> listsanat = new List<Sanatorium>();
            listsanat = db.Sanatorium.OrderBy(y => y.Name).ToList();
            ViewBag.listsanat = listsanat;

            List<Sanatorium> listKontr = new List<Sanatorium>();
            listKontr = db.Sanatorium.Where(t => t.Priznak == "ТурОператор" || t.Priznak == "Санаторий и ТурОператор").OrderBy(y => y.Name).ToList();
            ViewBag.listKontr = listKontr;
                        
            //Переделал вывод списка санаториев чтобы выводились только те, с которыми заключен договор
            List<Dogovor> ListDog = new List<Dogovor>();
            ListDog = db.Dogovor.Include(g => g.Sanatorium).Where(i => i.FillialId == userLog.Employee.FilialId).ToList();
            var ld = ListDog.GroupBy(h => h.Sanatorium).Select(grt => new { Name = grt.Key.Name, id = grt.Key.Id, spis = grt.Select(p => p) }).OrderBy(h => h.Name);
            ViewBag.listDog = ld;

            List<TableZay> listtab = new List<TableZay>();
            listtab = db.TableZay.Where(uu => uu.ZayId == ID).ToList();
            ViewBag.listtab = listtab;

            List<Child> listchild = new List<Child>();
            listchild = db.Child.Where(t => t.EmployeeId == userLog.Employee.Id && (t.StatusId == 2 || t.StatusId == 4 || t.StatusId == 9)).ToList();
            ViewBag.listchild = listchild;

            Zayavlenie zayEd = new Zayavlenie();
            zayEd = db.Zayavlenie.Include(g => g.Sanatorium).FirstOrDefault(a => a.Id == ID);

            return PartialView(zayEd);
        }
        //-------------------------------//

        //Сохранение редактирования заявления------------//
        [HttpPost]
        public ActionResult ZayEditSave([FromBody] ZayTab modus)
        {
            try
            {
                User u = new User();
                u = db.User.FirstOrDefault(g => g.Name == modus.UserMod);

                //Находим ID филиала пользователя
                Employee EMPL = new Employee();
                EMPL = db.Employee.FirstOrDefault(h => h.Id == u.EmployeeId);

                Zayavlenie zayE = new Zayavlenie();
                zayE = db.Zayavlenie.FirstOrDefault(s => s.Id == modus.Id);

                zayE.NumberZ = Convert.ToInt32(modus.NumberZ);
                zayE.DateZ = modus.DateZ;
                zayE.ProtocolId = modus.ProtocolId;
                zayE.EmployeeId = modus.EmployeeId;
                zayE.SanatoriumId = modus.SanatoriumId;
                if(modus.TurOpeId == 0)
                {
                    zayE.TurOpeId = null;
                }
                else
                {
                 zayE.TurOpeId = modus.TurOpeId;
                }                
                zayE.S = modus.S;
                zayE.Po = modus.Po;
                zayE.Who = modus.Who;
                zayE.PriznakOplata = modus.PriznakOplata;
                zayE.Priznak = modus.Priznak;
                zayE.Anulirovano = modus.Anulirovano;
                zayE.Summa = modus.Summa;
                zayE.SummaDop = modus.SummaDop;
                zayE.UserMod = modus.UserMod;
                zayE.DateMod = DateTime.Now;
                db.SaveChanges();

                //Работаем с таблицей TableZay


                if ((modus.Who.Trim() == "детей") || (modus.Who.Trim() == "семейная"))
                {

                    //Сперва удаляем записи с нашим ID в таблице TableZay
                    List<TableZay> TZ = new List<TableZay>();
                    TZ = db.TableZay.Where(h => h.ZayId == modus.Id).ToList();

                    db.TableZay.RemoveRange(TZ);
                    db.SaveChanges();
                    //-----------------------------------------

                    foreach (var item in modus.TableZ)
                    {
                        TableZay ZSD = new TableZay();
                        ZSD.ZayId = modus.Id;
                        ZSD.ChildId = item;

                        db.TableZay.Add(ZSD);
                        db.SaveChanges();
                    }
                }
                //Работаем с таблицей SUMDOGZAY

                //Сперва удаляем записи с нашим ID в таблице TableZay
                List<SummDogZay> SZ = new List<SummDogZay>();
                SZ = db.SummDogZay.Where(h => h.ZayavlenieId == modus.Id).ToList();
                db.SummDogZay.RemoveRange(SZ);
                db.SaveChanges();
                //-----------------------------------------
                //Получаем список договоров данного филлиала для списка санаториев с которыми заключен договор
                List<Dogovor> ListDog1 = new List<Dogovor>();
                ListDog1 = db.Dogovor.Where(i => i.FillialId == EMPL.FilialId && i.SanatoriumId == modus.SanatoriumId).ToList();

                //Находим ID последней записи (только что добавленную запись) в заявлениях
                List<Zayavlenie> listz = new List<Zayavlenie>();
                listz = db.Zayavlenie.OrderByDescending(g => g.DateMod).ToList();
                int ZAYID = listz.FirstOrDefault().Id;


                foreach (var item2 in ListDog1)
                {
                    SummDogZay SDZ = new SummDogZay();
                    SDZ.ZayavlenieId = ZAYID;
                    //SDZ.SanatoriumId = Convert.ToInt32(modus.SanatoriumId);
                    SDZ.DogovorId = item2.Id;
                    if (item2.TypeDogId == 1)
                    {
                        SDZ.Summa = modus.Summa;
                    }
                    if (item2.TypeDogId == 2)
                    {
                        SDZ.Summa = modus.SummaDop;
                    }
                    if (item2.TypeDogId == 3)
                    {
                        SDZ.Summa = modus.Summa + modus.SummaDop;
                    }

                    db.SummDogZay.Add(SDZ);
                    db.SaveChanges();
                }


                ViewBag.Message = "Заявление изменено";
            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//
        //---------------Работаем с заявлениями по разовому договору------------------------
        //----------Добавление заявления-----------------------//
        public ActionResult AddZay1()
        {
            var us = HttpContext.User.Identity?.Name;
            User userLog = new User();
            userLog = db.User.Include(g => g.Employee.Filial).FirstOrDefault(h => h.Name == us);

            List<Protocol> listprot = new List<Protocol>();
            listprot = db.Protocol.OrderByDescending(y => y.DateProt).Where(i => i.FilialId == userLog.Employee.FilialId).ToList();
            ViewBag.listprot = listprot;

            List<Employee> listempl = new List<Employee>();
            listempl = db.Employee.OrderBy(y => y.FirstName).Where(i => i.FilialId == userLog.Employee.FilialId).ToList();
            ViewBag.listempl = listempl;

            List<Sanatorium> listsanat = new List<Sanatorium>();
            listsanat = db.Sanatorium.Where(t =>t.Priznak =="Санаторий" || t.Priznak == "Санаторий и ТурОператор").OrderBy(y => y.Name).ToList();
            ViewBag.listsanat = listsanat;

            List<Sanatorium> listKontr = new List<Sanatorium>();
            listKontr = db.Sanatorium.Where(t => t.Priznak == "ТурОператор" || t.Priznak == "Санаторий и ТурОператор").OrderBy(y => y.Name).ToList();
            ViewBag.listKontr = listKontr;

            //Получаем список договоров данного филлиала для списка санаториев с которыми заключен договор
            List<Dogovor> ListDog = new List<Dogovor>();
            ListDog = db.Dogovor.Include(g => g.Sanatorium).Where(i => i.FillialId == userLog.Employee.FilialId).ToList();
            var ld = ListDog.GroupBy(h => h.Sanatorium).Select(grt => new { Name = grt.Key.Name, id = grt.Key.Id, spis = grt.Select(p => p) }).OrderBy(h => h.Name);
            ViewBag.listDog = ld;

            //Получим номер заявления по порядку в текущем году

            var usID = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<Zayavlenie> LZ = new List<Zayavlenie>();
            LZ = db.Zayavlenie.Where(i => i.Protocol.FilialId == UsFil.Employee.Filial.Id).OrderByDescending(u => u.DateZ).OrderByDescending(h => h.NumberZ).ToList();

            //Получаем последнюю запись в списке протоколов
            Zayavlenie ZEnd = new Zayavlenie();
            ZEnd = LZ.FirstOrDefault();

            //Получаем последний номер протокола
            int? numberZay = 0;
            if (LZ.Count == 0)
            {
                numberZay = 1;
            }
            else if (DateTime.Now.Year == Convert.ToDateTime(ZEnd.DateZ).Year)
            {
                numberZay = ZEnd.NumberZ + 1;
            }
            else if (DateTime.Now.Year == Convert.ToDateTime(ZEnd.DateZ).Year)
            {
                numberZay = 1;
            }

            ViewBag.numberZay = numberZay;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления заявления-----------//
        [HttpPost]
        public ActionResult ZaySave1([FromBody] ZayTab mod)
        {

            try
            {
                User u = new User();
                u = db.User.FirstOrDefault(g => g.Name == mod.UserMod);

                //Находим ID филиала пользователя
                Employee EMPL = new Employee();
                EMPL = db.Employee.FirstOrDefault(h => h.Id == u.EmployeeId);

                Zayavlenie za = new Zayavlenie();
                za.NumberZ = Convert.ToInt32(mod.NumberZ);
                za.DateZ = mod.DateZ;
                za.ProtocolId = mod.ProtocolId;
                za.EmployeeId = mod.EmployeeId;
                za.SanatoriumId = mod.SanatoriumId;
                za.S = mod.S;
                za.Po = mod.Po;
                za.Who = mod.Who;
                za.Summa = mod.Summa;
                za.SummaDop = mod.SummaDop;
                za.NumberDog = mod.NumberDog;
                za.DateDog = mod.DateDog;
                za.TurOpeId = mod.TurOpeId;
                za.UserMod = mod.UserMod;
                za.Priznak = "не принято";
                za.PriznakOplata = 0;
                za.Anulirovano = 0;
                za.DateMod = DateTime.Now;
                db.Zayavlenie.Add(za);
                db.SaveChanges();

                //Работаем с таблицей TableZay
                if ((mod.Who.Trim() == "детей") || (mod.Who.Trim() == "семейная"))
                {
                    //Находим ID последней записи (только что добавленную запись) в заявлениях
                    List<Zayavlenie> listz1 = new List<Zayavlenie>();
                    listz1 = db.Zayavlenie.OrderByDescending(g => g.DateMod).ToList();
                    int ZAYID1 = listz1.FirstOrDefault().Id;

                    foreach (var item in mod.TableZ)
                    {
                        TableZay ZSD = new TableZay();
                        ZSD.ZayId = ZAYID1;
                        ZSD.ChildId = item;

                        db.TableZay.Add(ZSD);
                        db.SaveChanges();
                    }
                }
                //Работаем с таблицей SUMDOGZAY

                //Получаем список договоров данного филлиала для списка санаториев с которыми заключен договор
                List<Dogovor> ListDog = new List<Dogovor>();
                ListDog = db.Dogovor.Where(i => i.FillialId == EMPL.FilialId && i.SanatoriumId == mod.SanatoriumId).ToList();

                //Находим ID последней записи (только что добавленную запись) в заявлениях
                List<Zayavlenie> listz = new List<Zayavlenie>();
                listz = db.Zayavlenie.OrderByDescending(g => g.DateMod).ToList();
                int ZAYID = listz.FirstOrDefault().Id;

                foreach (var item1 in ListDog)
                {
                    SummDogZay SDZ = new SummDogZay();
                    SDZ.ZayavlenieId = ZAYID;
                    // SDZ.SanatoriumId = Convert.ToInt32(mod.SanatoriumId);
                    SDZ.DogovorId = item1.Id;
                    if (item1.TypeDogId == 1)
                    {
                        SDZ.Summa = mod.Summa;
                    }
                    if (item1.TypeDogId == 2)
                    {
                        SDZ.Summa = mod.SummaDop;
                    }
                    if (item1.TypeDogId == 3)
                    {
                        SDZ.Summa = mod.Summa + mod.SummaDop;
                    }

                    db.SummDogZay.Add(SDZ);
                    db.SaveChanges();
                }


                ViewBag.Message = "Заявление успешно сформировано!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }
                
        // Редактирование заявления//

        public ActionResult ZayEdit1(int ID)
        {
            var us = HttpContext.User.Identity?.Name;
            User userLog = new User();
            userLog = db.User.Include(g => g.Employee.Filial).FirstOrDefault(h => h.Name == us);

            List<Protocol> listprot = new List<Protocol>();
            listprot = db.Protocol.OrderByDescending(y => y.DateProt).Where(i => i.FilialId == userLog.Employee.FilialId).ToList();
            ViewBag.listprot = listprot;

            List<Employee> listempl = new List<Employee>();
            listempl = db.Employee.OrderBy(y => y.FirstName).Where(i => i.FilialId == userLog.Employee.FilialId).ToList();
            ViewBag.listempl = listempl;

            List<Sanatorium> listsanat = new List<Sanatorium>();
            listsanat = db.Sanatorium.Where(t => t.Priznak == "Санаторий" || t.Priznak == "Санаторий и ТурОператор").OrderBy(y => y.Name).ToList();
            ViewBag.listsanat = listsanat;

            List<Sanatorium> listKontr = new List<Sanatorium>();
            listKontr = db.Sanatorium.Where(t => t.Priznak == "ТурОператор" || t.Priznak == "Санаторий и ТурОператор").OrderBy(y => y.Name).ToList();
            ViewBag.listKontr = listKontr;

            List<TableZay> listtab = new List<TableZay>();
            listtab = db.TableZay.Where(uu => uu.ZayId == ID).ToList();
            ViewBag.listtab = listtab;

            List<Child> listchild = new List<Child>();
            listchild = db.Child.Where(t => t.EmployeeId == userLog.Employee.Id && (t.StatusId == 2 || t.StatusId == 4 || t.StatusId == 9)).ToList();
            ViewBag.listchild = listchild;

            Zayavlenie zayEd = new Zayavlenie();
            zayEd = db.Zayavlenie.Include(g => g.Sanatorium).FirstOrDefault(a => a.Id == ID);

            return PartialView(zayEd);
        }
        //-------------------------------//

        //Сохранение редактирования заявления------------//
        [HttpPost]
        public ActionResult ZayEditSave1([FromBody] ZayTab modus)
        {
            try
            {
                User u = new User();
                u = db.User.FirstOrDefault(g => g.Name == modus.UserMod);

                //Находим ID филиала пользователя
                Employee EMPL = new Employee();
                EMPL = db.Employee.FirstOrDefault(h => h.Id == u.EmployeeId);

                Zayavlenie zayE = new Zayavlenie();
                zayE = db.Zayavlenie.FirstOrDefault(s => s.Id == modus.Id);

                zayE.NumberZ = Convert.ToInt32(modus.NumberZ);
                zayE.DateZ = modus.DateZ;
                zayE.ProtocolId = modus.ProtocolId;
                zayE.EmployeeId = modus.EmployeeId;
                zayE.SanatoriumId = modus.SanatoriumId;
                zayE.S = modus.S;
                zayE.Po = modus.Po;
                zayE.Who = modus.Who;
                zayE.TurOpeId = modus.TurOpeId;
                zayE.NumberDog = modus.NumberDog;
                zayE.DateDog = modus.DateDog;
                zayE.Summa = modus.Summa;
                zayE.SummaDop = modus.SummaDop;
                zayE.UserMod = modus.UserMod;
                zayE.DateMod = DateTime.Now;
                db.SaveChanges();

                //Работаем с таблицей TableZay


                if ((modus.Who.Trim() == "детей") || (modus.Who.Trim() == "семейная"))
                {

                    //Сперва удаляем записи с нашим ID в таблице TableZay
                    List<TableZay> TZ = new List<TableZay>();
                    TZ = db.TableZay.Where(h => h.ZayId == modus.Id).ToList();

                    db.TableZay.RemoveRange(TZ);
                    db.SaveChanges();
                    //-----------------------------------------

                    foreach (var item in modus.TableZ)
                    {
                        TableZay ZSD = new TableZay();
                        ZSD.ZayId = modus.Id;
                        ZSD.ChildId = item;

                        db.TableZay.Add(ZSD);
                        db.SaveChanges();
                    }
                }
                //Работаем с таблицей SUMDOGZAY

                //Сперва удаляем записи с нашим ID в таблице TableZay
                List<SummDogZay> SZ = new List<SummDogZay>();
                SZ = db.SummDogZay.Where(h => h.ZayavlenieId == modus.Id).ToList();
                db.SummDogZay.RemoveRange(SZ);
                db.SaveChanges();
                //-----------------------------------------
                //Получаем список договоров данного филлиала для списка санаториев с которыми заключен договор
                List<Dogovor> ListDog1 = new List<Dogovor>();
                ListDog1 = db.Dogovor.Where(i => i.FillialId == EMPL.FilialId && i.SanatoriumId == modus.SanatoriumId).ToList();

                //Находим ID последней записи (только что добавленную запись) в заявлениях
                List<Zayavlenie> listz = new List<Zayavlenie>();
                listz = db.Zayavlenie.OrderByDescending(g => g.DateMod).ToList();
                int ZAYID = listz.FirstOrDefault().Id;


                foreach (var item2 in ListDog1)
                {
                    SummDogZay SDZ = new SummDogZay();
                    SDZ.ZayavlenieId = ZAYID;
                    //SDZ.SanatoriumId = Convert.ToInt32(modus.SanatoriumId);
                    SDZ.DogovorId = item2.Id;
                    if (item2.TypeDogId == 1)
                    {
                        SDZ.Summa = modus.Summa;
                    }
                    if (item2.TypeDogId == 2)
                    {
                        SDZ.Summa = modus.SummaDop;
                    }
                    if (item2.TypeDogId == 3)
                    {
                        SDZ.Summa = modus.Summa + modus.SummaDop;
                    }

                    db.SummDogZay.Add(SDZ);
                    db.SaveChanges();
                }


                ViewBag.Message = "Заявление изменено";
            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//
        //----------------------------------------------------------------------------------
        // СПИСОК детей в заявлении//
        public ActionResult GetChildren(int ID)
        {
            IEnumerable<Child> childrenlList = db.Child;
            childrenlList = db.Child.OrderBy(y => y.Fio).Where(a => a.EmployeeId == ID && (a.StatusId == 2 || a.StatusId == 4 || a.StatusId == 9)).ToList();
            ViewBag.children = childrenlList;

            List<Child> child = new List<Child>();
            child = db.Child.OrderBy(x => x.Fio).Where(a => a.EmployeeId == ID).ToList();
            if (child.Count() == 0)
            {
                ViewBag.Message = "У данного работника нет детей!!!";
                return PartialView();
            }
            else
            {
                return PartialView(child);
            }
        }
        //--------------------------//
        // СПИСОК детей в заявлении//
        public ActionResult GetFamily(int ID)
        {
            IEnumerable<Child> familylList = db.Child;
            familylList = db.Child.OrderBy(y => y.Fio).Where(a => a.EmployeeId == ID).ToList();
            ViewBag.family = familylList;

            List<Child> child = new List<Child>();
            child = db.Child.OrderBy(x => x.Fio).Where(a => a.EmployeeId == ID).ToList();
            if (child.Count() == 0)
            {
                ViewBag.Message = "У данного работника нет родственников!!!";
                return PartialView();
            }
            else
            {
                return PartialView(child);
            }
        }
        //--------------------------//
        public ActionResult GetChildrenEdit(int ID)
        {
            //находим изменяемое заявление по ID
            Zayavlenie Za = new Zayavlenie();
            Za = db.Zayavlenie.FirstOrDefault(f => f.Id == ID);
            //------------------------------------------------

            IEnumerable<Child> childrenlList = db.Child;
            childrenlList = db.Child.OrderBy(y => y.Fio).Where(a => a.EmployeeId == Za.EmployeeId && (a.StatusId == 2 || a.StatusId == 4 || a.StatusId == 9)).ToList();
            ViewBag.children = childrenlList;

            List<TableZay> listtab = new List<TableZay>();
            listtab = db.TableZay.Where(uu => uu.ZayId == ID).ToList();
            ViewBag.listtab = listtab;

            List<Child> child = new List<Child>();
            child = db.Child.OrderBy(x => x.Fio).Where(a => a.EmployeeId == ID).ToList();
            if (child.Count() == 0)
            {
                ViewBag.Message = "У данного работника нет детей!!!";
                return PartialView();
            }
            else
            {
                return PartialView(child);
            }
        }


        //Сохранение закрытия заявления------------//
        public ActionResult PutZay(int ID)
        {
            try
            {
                Zayavlenie ZPUT = new Zayavlenie();
                ZPUT = db.Zayavlenie.FirstOrDefault(s => s.Id == ID);

                ZPUT.Priznak = "принято";
                ZPUT.UserMod = HttpContext.User.Identity.Name;
                ZPUT.DateMod = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "Заявление обработано";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }

        //Редактирование заявления когда путевка семейная
        public ActionResult GetFamilyEdit(int ID)
        {
            //находим изменяемое заявление по ID
            Zayavlenie Za = new Zayavlenie();
            Za = db.Zayavlenie.FirstOrDefault(f => f.Id == ID);
            //------------------------------------------------

            IEnumerable<Child> childrenlList = db.Child;
            childrenlList = db.Child.Where(a => a.EmployeeId == Za.EmployeeId).OrderBy(y => y.Fio).ToList();
            ViewBag.children = childrenlList;

            List<TableZay> listtab = new List<TableZay>();
            listtab = db.TableZay.Where(uu => uu.ZayId == ID).ToList();
            ViewBag.listtab = listtab;

            List<Child> child = new List<Child>();
            child = db.Child.OrderBy(x => x.Fio).Where(a => a.EmployeeId == ID).ToList();
            if (child.Count() == 0)
            {
                ViewBag.Message = "У данного работника нет родственников!!!";
                return PartialView();
            }
            else
            {
                return PartialView(child);
            }
        }
        //--------------------КОМИССИЯ------------------------------------------------------------------
        //----------Добавление комиссии-----------------------//

        public ActionResult AddKomissiya()
        {
            var phone = HttpContext.User.FindAll(ClaimTypes.Role);

            List<Employee> ListEmpl = new List<Employee>();

            foreach (var item in phone)
            {
                ListEmpl.AddRange(db.Employee.Include(q => q.Filial).Include(o => o.Department).Include(oo => oo.Position).Where(i => i.FilialId == Convert.ToInt32(item.Value)).OrderBy(d => d.FirstName).ToList());
            }
            ViewBag.listempl = ListEmpl;

            List<Naznach> Listnazn = new List<Naznach>();
            Listnazn = db.Naznach.OrderBy(h => h.Priznak).ToList();
            ViewBag.Listnazn = Listnazn;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления комиссии-----------//
        [HttpPost]
        public ActionResult KomissiyaSave([FromBody] Komissiya mod)
        {

            try
            {
                Komissiya us = new Komissiya();
                us.EmployeeId = mod.EmployeeId;
                us.StatusId = mod.StatusId;
                us.UserModific = mod.UserModific;
                us.DateModific = DateTime.Now;
                db.Komissiya.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Член успешно добавлен!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление комиссии//

        public ActionResult DeleteKomissiya(int ID)
        {
            Komissiya komissiyaDel = new Komissiya();
            komissiyaDel = db.Komissiya.Include(h => h.Employee).FirstOrDefault(a => a.Id == ID);
            return PartialView(komissiyaDel);
        }
        //-----------------------------//

        // Подтверждение удаления комиссии//
        public ActionResult DeleteKomissiyaOK(int ID)
        {
            try
            {
                Komissiya komissiyaDS = new Komissiya();
                komissiyaDS = db.Komissiya.FirstOrDefault(a => a.Id == ID);
                db.Komissiya.Remove(komissiyaDS);
                db.SaveChanges();

                ViewBag.Message = "Член удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование комиссии//

        public ActionResult KomissiyaEdit(int ID)
        {
            var phone = HttpContext.User.FindAll(ClaimTypes.Role);
            List<Employee> ListEmpl = new List<Employee>();
            foreach (var item in phone)
            {
                ListEmpl.AddRange(db.Employee.Include(q => q.Filial).Include(o => o.Department).Include(oo => oo.Position).Where(i => i.FilialId == Convert.ToInt32(item.Value)).OrderBy(d => d.FirstName).ToList());
            }
            ViewBag.listempl = ListEmpl;

            List<Naznach> Listnazn = new List<Naznach>();
            Listnazn = db.Naznach.ToList();
            ViewBag.Listnazn = Listnazn;

            Komissiya UserEd = new Komissiya();
            UserEd = db.Komissiya.FirstOrDefault(a => a.Id == ID);

            return PartialView(UserEd);
        }
        //-------------------------------//

        //Сохранение редактирования комиссии------------//
        [HttpPost]
        public ActionResult KomissiyaEditSave([FromBody] Komissiya modus)
        {
            try
            {

                //Если данного пользователя делают подписантом, обнуляем у всего списка Priznak
                if (modus.Priznak == "1")
                {
                    List<Komissiya> ListEmpl = new List<Komissiya>();
                    var usID = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    User UsFil = new User();
                    UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    ListEmpl = db.Komissiya.Include(o => o.Employee.Position).Include(oo => oo.Employee.Department).Include(ww => ww.Status).Where(i => i.Employee.FilialId == UsFil.Employee.FilialId).OrderBy(h => h.StatusId).ToList();

                    foreach (var i in ListEmpl)
                    {
                        i.Priznak = null;
                    }
                }
                //-----------------------------------------------------------------------------

                Komissiya komissiyaE = new Komissiya();
                komissiyaE = db.Komissiya.FirstOrDefault(s => s.Id == modus.Id);

                komissiyaE.StatusId = modus.StatusId;
                komissiyaE.EmployeeId = modus.EmployeeId;
                komissiyaE.Priznak = modus.Priznak;
                komissiyaE.UserModific = modus.UserModific;
                komissiyaE.DateModific = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "Член изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //--------Выводим на печать протокол со списком сотрудников---------------------
        //public ActionResult PrintProtocol(string ID)
        //{
        //    //Находим список заявлений соглассно выбранного протокола
        //    List<Zayavlenie> LZ = new List<Zayavlenie>();
        //    LZ = db.Zayavlenie.Where(g => g.ProtocolId == Convert.ToInt32(ID)).ToList();

        //    //Находим протокол
        //    Protocol prot = new Protocol();
        //    prot = db.Protocol.FirstOrDefault(t => t.Id == Convert.ToInt32(ID));

        //    Application app = new Application();
        //    Document doc = app.Documents.Add(Visible: true);
        //    Microsoft.Office.Interop.Word.Range r = doc.Range();
        //    r.ParagraphFormat.LineSpacing = 12;
        //    r.Font.Size = 14;
        //    r.Font.Name = "Times New Roman";
        //    r.ParagraphFormat.SpaceBefore = 1;
        //    r.ParagraphFormat.SpaceAfter = 1;

        //    //------------------------------------------
        //    var Paragraph4 = app.ActiveDocument.Paragraphs.Add();
        //    var a = Paragraph4.Range;
        //    a.Text = "ОАО \"Гомельтранснефть Дружба\"\n";
        //    a.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        //    a.Font.Underline = WdUnderline.wdUnderlineSingle;
        //    a.ParagraphFormat.SpaceAfter = 10;

        //    var ParagrapQQ = app.ActiveDocument.Paragraphs.Add();
        //    var ssQQ = ParagrapQQ.Range;
        //    ssQQ.Text = "Протокол № " + prot.NumberP + " от " + Convert.ToDateTime(prot.DateProt).ToString("d");
        //    ssQQ.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;

        //    r.InsertAfter("\n\n");
        //    //-------------------------------------------------------------------------------------------------------
        //    var Paragraph2 = app.ActiveDocument.Paragraphs.Add();
        //    var tableRange2 = Paragraph2.Range;
        //    tableRange2.Font.Size = 14;
        //    Table tOsttest2 = doc.Tables.Add(tableRange2, LZ.Count + 2, 7);
        //    tOsttest2.Borders.Enable = 1;
        //    tOsttest2.Rows.Add();
        //    tOsttest2.Rows[1].Cells[1].Range.Text = "п/п";
        //    tOsttest2.Rows[1].Cells[2].Range.Text = "Номер заявления";
        //    tOsttest2.Rows[1].Cells[3].Range.Text = "Дата заявления";
        //    tOsttest2.Rows[1].Cells[4].Range.Text = "Сотрудник";
        //    tOsttest2.Rows[1].Cells[5].Range.Text = "На кого";
        //    tOsttest2.Rows[1].Cells[6].Range.Text = "С";
        //    tOsttest2.Rows[1].Cells[7].Range.Text = "По";

        //    tOsttest2.Rows[1].Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        //    tOsttest2.Rows[1].Cells[1].Range.Font.Size = 12;
        //    tOsttest2.Rows[1].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        //    tOsttest2.Rows[1].Cells[2].Range.Font.Size = 12;
        //    tOsttest2.Rows[1].Cells[3].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        //    tOsttest2.Rows[1].Cells[3].Range.Font.Size = 12;
        //    tOsttest2.Rows[1].Cells[4].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        //    tOsttest2.Rows[1].Cells[4].Range.Font.Size = 12;
        //    tOsttest2.Rows[1].Cells[5].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        //    tOsttest2.Rows[1].Cells[5].Range.Font.Size = 12;
        //    tOsttest2.Rows[1].Cells[6].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        //    tOsttest2.Rows[1].Cells[6].Range.Font.Size = 12;
        //    tOsttest2.Rows[1].Cells[7].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
        //    tOsttest2.Rows[1].Cells[7].Range.Font.Size = 12;

        //    int index = 1;
        //    foreach (var y in LZ)
        //    {
        //        index++;
        //        tOsttest2.Rows[index].Cells[1].Range.Text = index.ToString();
        //        tOsttest2.Rows[index].Cells[2].Range.Text = y.NumberZ.ToString();
        //        tOsttest2.Rows[index].Cells[3].Range.Text = Convert.ToDateTime(y.DateZ).ToString("d");
        //        tOsttest2.Rows[index].Cells[4].Range.Text = y.Employee.FirstName + y.Employee.LastName;
        //        tOsttest2.Rows[index].Cells[5].Range.Text = y.Who;
        //        tOsttest2.Rows[index].Cells[6].Range.Text = Convert.ToDateTime(y.S).ToString("d");
        //        tOsttest2.Rows[index].Cells[7].Range.Text = Convert.ToDateTime(y.Po).ToString("d");
        //        tOsttest2.Rows[index].Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
        //        tOsttest2.Rows[index].Cells[1].Range.Font.Size = 10;
        //        tOsttest2.Rows[index].Cells[2].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
        //        tOsttest2.Rows[index].Cells[2].Range.Font.Size = 10;
        //        tOsttest2.Rows[index].Cells[3].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
        //        tOsttest2.Rows[index].Cells[3].Range.Font.Size = 10;
        //        tOsttest2.Rows[index].Cells[4].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
        //        tOsttest2.Rows[index].Cells[4].Range.Font.Size = 10;
        //        tOsttest2.Rows[index].Cells[5].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
        //        tOsttest2.Rows[index].Cells[5].Range.Font.Size = 10;
        //        tOsttest2.Rows[index].Cells[6].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
        //        tOsttest2.Rows[index].Cells[6].Range.Font.Size = 10;
        //        tOsttest2.Rows[index].Cells[7].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
        //        tOsttest2.Rows[index].Cells[7].Range.Font.Size = 10;
        //    }
        //    //-------------------------Вывод итогов нефтепродуктов по месяцам--------------------------------


        //    MemoryStream stream = new MemoryStream();

        //    string strPDFFileName = string.Format("Report.docx");

        //    byte[] array = System.IO.File.ReadAllBytes(@"C:\Install\Report.docx");


        //    stream.Write(array, 0, array.Length);
        //    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "ReportRegistration.docx");


        //}

        //----------------------------------------------------------------------------------
        public ActionResult Print(int ID)
        {
            Protocol pr = new Protocol();
            pr = db.Protocol.Include(t => t.Filial).FirstOrDefault(g => g.Id == ID);

            List<Zayavlenie> listZay = new List<Zayavlenie>();
            listZay = db.Zayavlenie.Include(u => u.Employee).Include(y => y.Sanatorium.City).Where(f => f.ProtocolId == ID).ToList();

            //-Сгрупируем список по Работнику, чтобы можно было вывести список детей тоже---------------------------------------
            //List<Zayavlenie> listZay1 = new List<Zayavlenie>();
            //var listZay2 = (from zayav in db.Zayavlenie
            //               join tableZay in db.TableZay on zayav.Id equals tableZay.ZayId/* into RoGroup
            //from subRo in RoGroup.DefaultIfEmpty()*/
            //               join child in db.Child on tableZay.ChildId equals child.Id
            //               where zayav.ProtocolId == ID
            //               select new
            //               {
            //                   Zayav = zayav.Employee,
            //                   TableZay = tableZay,
            //                   Child = child
            //               }).GroupBy(hj =>hj.Zayav);

            //----------------------------
            var GroupList = listZay.GroupJoin(db.TableZay.Include(j => j.Child),
                c => c.Id,
                p => p.ZayId,
                (c, key) => new
                {
                    employee = c.Employee.FirstName + " " + c.Employee.LastName.Substring(0, 1) + " " + c.Employee.MiddleName.Substring(0, 1),
                    sanatorium = c.Sanatorium.Name,
                    s = c.S,
                    po = c.Po,
                    who = c.Who,
                    city = c.Sanatorium.City.Name,
                    kol = (Convert.ToDateTime(c.Po) - Convert.ToDateTime(c.S)).Days + 1,
                    child = key.Select(p => p.Child),
                    count = key.Count()
                 }
                );

            var ListZayGroup = listZay.GroupBy(jk => jk.Employee).Select(df => new { employee = df.Key.FirstName + df.Key.LastName + df.Key.MiddleName, spis = df.Select(p => p) });
                                   
            List<Komissiya> listkom = new List<Komissiya>();
            listkom = db.Komissiya.Include(h => h.Status).Include(w => w.Employee.Position).OrderBy(u => u.Status.Priznak).Where(t => t.Employee.FilialId == pr.FilialId && t.StatusId != 4).ToList();

            string numdat = Convert.ToDateTime(pr.DateProt).ToString("d") + " № " + pr.NumberP + "\n" + pr.Filial.Priznak;

            Komissiya secretar = new Komissiya();
            if (listkom.FirstOrDefault(t => t.Priznak == "1") != null)
            {
                secretar = listkom.FirstOrDefault(t => t.Priznak == "1");
            }
            else
            {
                secretar = listkom.FirstOrDefault(j => j.Status.Name.Trim() == "Секретарь");
            }


            var newFile2 = @"C:/Install/Protocol.docx";
            //var newFile2 = @"wwwroot/Report/Protocol.docx";
            using (var fs = new FileStream(newFile2, FileMode.Create, FileAccess.Write))
            {
                XWPFDocument doc = new XWPFDocument();

                //----Заполняем шапку Протокола-----------------------------------------------------------------------------------------------------------------------------------------------------

                XWPFTable table1 = doc.CreateTable(3, 1);
                table1.Width = 2000;

                table1.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para1 = table1.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para2 = table1.GetRow(1).GetCell(0).Paragraphs[0];
                XWPFParagraph para3 = table1.GetRow(2).GetCell(0).Paragraphs[0];

                XWPFRun run1 = para1.CreateRun();
                run1.FontFamily = "Times New Roman";
                run1.FontSize = 12;
                run1.SetText("Открытое акционерное общество \"Гомельтранснефть Дружба\"");

                XWPFRun run2 = para2.CreateRun();
                run2.FontFamily = "Times New Roman";
                run2.FontSize = 12;
                run2.SetText("ПРОТОКОЛ");

                XWPFRun run3 = para3.CreateRun();
                run3.FontFamily = "Times New Roman";
                run3.FontSize = 12;
                run3.SetText(Convert.ToDateTime(pr.DateProt).ToString("d") + " № " + pr.NumberP);

                para3.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run3R = para3.CreateRun();
                run3R.FontFamily = "Times New Roman";
                run3R.FontSize = 12;
                if (pr.Filial.Priznak != null)
                {
                    run3R.SetText(pr.Filial.Priznak.ToString());
                }
                else
                {
                    run3R.SetText("");
                }

                //---------------------------------------------------------------------------------------------------------------------------------------------------------
                var p2 = doc.CreateParagraph();
                p2.Alignment = ParagraphAlignment.LEFT;
                p2.IndentationFirstLine = 500;
                XWPFRun r2 = p2.CreateRun();
                r2.FontFamily = "Times New Roman";
                r2.FontSize = 12;
                r2.IsBold = false;
                r2.SetText("Заседание комиссии по оздоровлению и санаторно-курортному лечению " + pr.Filial.Name + " ОАО \"Гомельтранснефть Дружба\"");

                //---------------------------------------------------------------------------------------------------------------------------------------------------------

                int index = -1;
                XWPFTable table3 = doc.CreateTable(listkom.Count, 3);
                table3.Width = 5000;

                table3.SetColumnWidth(0, 1500);
                table3.SetColumnWidth(1, 1500);
                table3.SetColumnWidth(2, 1500);

                table3.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                foreach (var item in listkom)
                {
                    index++;

                    XWPFParagraph para31 = table3.GetRow(index).GetCell(0).Paragraphs[0];
                    XWPFParagraph para32 = table3.GetRow(index).GetCell(1).Paragraphs[0];
                    XWPFParagraph para33 = table3.GetRow(index).GetCell(2).Paragraphs[0];

                    XWPFRun run31 = para31.CreateRun();
                    run31.FontFamily = "Times New Roman";
                    run31.FontSize = 12;
                    run31.SetText(item.Status.Name.Trim());

                    XWPFRun run32 = para32.CreateRun();
                    run32.FontFamily = "Times New Roman";
                    run32.FontSize = 12;
                    run32.SetText("\t\t\t\t\t\t" + "   ---   ");

                    XWPFRun run33 = para33.CreateRun();
                    run33.FontFamily = "Times New Roman";
                    run33.FontSize = 12;
                    run33.SetText("\t\t\t\t" + item.Employee.LastName.Substring(0, 1) + "." + item.Employee.MiddleName.Substring(0, 1) + "." + item.Employee.FirstName);

                }
                //---------------------------------------------------------------------------------------------------------------------------------------------------------
                var p001 = doc.CreateParagraph();
                p001.Alignment = ParagraphAlignment.LEFT;
                XWPFRun r001 = p001.CreateRun();
                r001.FontFamily = "Times New Roman";
                r001.FontSize = 12;
                r001.IsBold = false;
                r001.SetText("");

                var p3 = doc.CreateParagraph();
                p3.Alignment = ParagraphAlignment.LEFT;
                //p3.IndentationFirstLine = 500;
                XWPFRun r3 = p3.CreateRun();
                r3.FontFamily = "Times New Roman";
                r3.FontSize = 12;
                r3.IsBold = false;
                r3.SetText("ПОВЕСТКА ДНЯ:");

                var p4 = doc.CreateParagraph();
                p4.Alignment = ParagraphAlignment.BOTH;
                p4.IndentationFirstLine = 500;
                XWPFRun r4 = p4.CreateRun();
                r4.FontFamily = "Times New Roman";
                r4.FontSize = 12;
                r4.IsBold = false;
                r4.SetText("Рассмотрение заявлений работников и пенсионеров " + pr.Filial.Name + " \"Гомельтранснефть Дружба\" о согласовании выделения денежных средств на санаторно-курортное лечение и оздоровление согласно действующему Коллективному договору.");
                //---------------------------------------------------------------------------------------------------------------------------------------------------------

                var p11 = doc.CreateParagraph();
                p11.Alignment = ParagraphAlignment.LEFT;
                //p1.IndentationFirstLine = 500;
                XWPFRun r11 = p11.CreateRun();
                r11.FontFamily = "Times New Roman";
                r11.FontSize = 12;
                r11.IsBold = false;
                r11.SetText("");

                var p5 = doc.CreateParagraph();
                p5.Alignment = ParagraphAlignment.LEFT;
                //p3.IndentationFirstLine = 500;
                XWPFRun r5 = p5.CreateRun();
                r5.FontFamily = "Times New Roman";
                r5.FontSize = 12;
                r5.IsBold = false;
                r5.SetText("СЛУШАЛИ:");

                var p6 = doc.CreateParagraph();
                p6.Alignment = ParagraphAlignment.BOTH;
                p6.IndentationFirstLine = 500;
                XWPFRun r6 = p6.CreateRun();
                r6.FontFamily = "Times New Roman";
                r6.FontSize = 12;
                r6.IsBold = false;
                r6.SetText("Председателя комиссии по оздоровлению " + pr.Filial.Name + " по вопросу рассмотрения заявлений работников и пенсионеров " + pr.Filial.Name + " \"Гомельтранснефть Дружба\" о выделении денежных средств на санаторно-курортное лечение и оздоровление в соответствии с Разделом 7 действующего Коллективного договора.");
                //---------------------------------------------------------------------------------------------------------------------------------------------------------

                var p111 = doc.CreateParagraph();
                p111.Alignment = ParagraphAlignment.LEFT;
                //p111.IndentationFirstLine = 500;
                XWPFRun r111 = p111.CreateRun();
                r111.FontFamily = "Times New Roman";
                r111.FontSize = 12;
                r111.IsBold = false;
                r111.SetText("");

                var p7 = doc.CreateParagraph();
                p7.Alignment = ParagraphAlignment.LEFT;
                //p3.IndentationFirstLine = 500;
                XWPFRun r7 = p7.CreateRun();
                r7.FontFamily = "Times New Roman";
                r7.FontSize = 12;
                r7.IsBold = false;
                r7.SetText("РЕШИЛИ:");

                var p8 = doc.CreateParagraph();
                p8.Alignment = ParagraphAlignment.BOTH;
                p8.IndentationFirstLine = 500;
                XWPFRun r8 = p8.CreateRun();
                r8.FontFamily = "Times New Roman";
                r8.FontSize = 12;
                r8.IsBold = false;
                r8.SetText("Выделить денежные средства с последующей оплатой на санаторно-курортное лечение и оздоровление в соответствии с действующим Коллективным договором работникам и пенсионерам " + pr.Filial.Name + " \"Гомельтранснефть Дружба\", указанных в Приложении к протоколу.");

                //----Выводим таблицу комиссии с подписями-----------------------------------------------------------------------------------------------------------

                int ind = -1;
                XWPFTable table4 = doc.CreateTable(listkom.Count, 3);
                table4.Width = 5000;

                table4.SetColumnWidth(0, 1500);
                table4.SetColumnWidth(1, 1500);
                table4.SetColumnWidth(2, 1500);

                table4.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table4.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table4.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table4.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table4.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table4.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                foreach (var item1 in listkom)
                {
                    ind++;

                    XWPFParagraph para41 = table4.GetRow(ind).GetCell(0).Paragraphs[0];
                    XWPFParagraph para42 = table4.GetRow(ind).GetCell(1).Paragraphs[0];
                    XWPFParagraph para43 = table4.GetRow(ind).GetCell(2).Paragraphs[0];

                    XWPFRun run41 = para41.CreateRun();
                    run41.FontFamily = "Times New Roman";
                    run41.FontSize = 12;
                    run41.SetText(item1.Status.Name.Trim());

                    XWPFRun run42 = para42.CreateRun();
                    run42.FontFamily = "Times New Roman";
                    run42.FontSize = 12;
                    run42.SetText("\t\t\t" + "______________________");

                    XWPFRun run43 = para43.CreateRun();
                    run43.FontFamily = "Times New Roman";
                    run43.FontSize = 12;
                    run43.SetText("\t\t\t\t" + item1.Employee.LastName.Substring(0, 1) + "." + item1.Employee.MiddleName.Substring(0, 1) + "." + item1.Employee.FirstName);

                }

                var p110 = doc.CreateParagraph();
                p110.Alignment = ParagraphAlignment.LEFT;
                //p111.IndentationFirstLine = 500;
                XWPFRun r110 = p110.CreateRun();
                r110.FontFamily = "Times New Roman";
                r110.FontSize = 12;
                r110.IsBold = false;
                r110.SetText("");

                //-----------Заполняем второй лист протокола!!!!!!!-------------------------

                doc.CreateParagraph().CreateRun().AddBreak(BreakType.PAGE);

                XWPFTable table22 = doc.CreateTable(1, 2);
                table22.Width = 5000;
                table22.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table22.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table22.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table22.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table22.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table22.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para202 = table22.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFRun run202 = para202.CreateRun();
                run202.FontFamily = "Times New Roman";
                run202.FontSize = 12;
                run202.SetText("\t\t\t\t\t\t\t\t\t\t\t");

                XWPFParagraph para22 = table22.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFRun run22 = para22.CreateRun();
                run22.FontFamily = "Times New Roman";
                run22.FontSize = 12;
                run22.SetText("Приложение к Протоколу");
                para22.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run22R = para22.CreateRun();
                run22R.FontFamily = "Times New Roman";
                run22R.FontSize = 12;
                run22R.SetText("заседания комиссии по оздоровлению ");
                para22.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run22RR = para22.CreateRun();
                run22RR.FontFamily = "Times New Roman";
                run22RR.FontSize = 12;
                run22RR.SetText(Convert.ToDateTime(pr.DateProt).ToString("d") + " № " + pr.NumberP);

                //doc.SetParagraph(para22, 1);
                table22.SetColumnWidth(0, 2500);
                table22.SetColumnWidth(1, 2500);

                var p1110 = doc.CreateParagraph();
                p1110.Alignment = ParagraphAlignment.LEFT;
                //p111.IndentationFirstLine = 500;
                XWPFRun r1110 = p1110.CreateRun();
                r1110.FontFamily = "Times New Roman";
                r1110.FontSize = 12;
                r1110.IsBold = false;
                r1110.SetText("");

                //--------Добавляем на второй лист таблицу с заявлениями--------------------
                //----------это раскоментировать----------------------------------------------------------------------
                //int ind2 = 0;
                //XWPFTable table221 = doc.CreateTable(listZay.Count + 1, 6);
                //table221.Width = 5000;

                //--Заполняю шапку таблицы----------------------------------------------------

                //XWPFParagraph para22110 = table221.GetRow(0).GetCell(0).Paragraphs[0];
                //XWPFParagraph para22120 = table221.GetRow(0).GetCell(1).Paragraphs[0];
                //XWPFParagraph para22130 = table221.GetRow(0).GetCell(2).Paragraphs[0];
                //XWPFParagraph para22140 = table221.GetRow(0).GetCell(3).Paragraphs[0];
                //XWPFParagraph para22150 = table221.GetRow(0).GetCell(4).Paragraphs[0];
                //XWPFParagraph para22160 = table221.GetRow(0).GetCell(5).Paragraphs[0];

                //XWPFRun run22110 = para22110.CreateRun();
                //para22110.Alignment = ParagraphAlignment.CENTER;
                //run22110.FontFamily = "Times New Roman";
                //run22110.FontSize = 12;
                //run22110.SetText("№ п/п");

                //XWPFRun run22120 = para22120.CreateRun();
                //para22120.Alignment = ParagraphAlignment.CENTER;
                //run22120.FontFamily = "Times New Roman";
                //run22120.FontSize = 12;
                //run22120.SetText("ФИО работника/ пенсионера");

                //XWPFRun run22130 = para22130.CreateRun();
                //para22130.Alignment = ParagraphAlignment.CENTER;
                //run22130.FontFamily = "Times New Roman";
                //run22130.FontSize = 12;
                //run22130.SetText("Место оздоровления");

                //XWPFRun run22140 = para22140.CreateRun();
                //para22140.Alignment = ParagraphAlignment.CENTER;
                //run22140.FontFamily = "Times New Roman";
                //run22140.FontSize = 12;
                //run22140.SetText("Дата С");

                //XWPFRun run22150 = para22150.CreateRun();
                //para22150.Alignment = ParagraphAlignment.CENTER;
                //run22150.FontFamily = "Times New Roman";
                //run22150.FontSize = 12;
                //run22150.SetText("Дата По");

                //XWPFRun run22160 = para22160.CreateRun();
                //para22160.Alignment = ParagraphAlignment.CENTER;
                //run22160.FontFamily = "Times New Roman";
                //run22160.FontSize = 12;
                //run22160.SetText("Кол-во дней");

                //foreach (var item2 in listZay)
                //{
                //    ind2++;
                //    double g = (Convert.ToDateTime(item2.Po) - Convert.ToDateTime(item2.S)).TotalDays + 1;

                //    XWPFParagraph para2211 = table221.GetRow(ind2).GetCell(0).Paragraphs[0];
                //    XWPFParagraph para2212 = table221.GetRow(ind2).GetCell(1).Paragraphs[0];
                //    XWPFParagraph para2213 = table221.GetRow(ind2).GetCell(2).Paragraphs[0];
                //    XWPFParagraph para2214 = table221.GetRow(ind2).GetCell(3).Paragraphs[0];
                //    XWPFParagraph para2215 = table221.GetRow(ind2).GetCell(4).Paragraphs[0];
                //    XWPFParagraph para2216 = table221.GetRow(ind2).GetCell(5).Paragraphs[0];

                //    XWPFRun run2211 = para2211.CreateRun();
                //    para2211.Alignment = ParagraphAlignment.CENTER;
                //    run2211.FontFamily = "Times New Roman";
                //    run2211.FontSize = 12;
                //    run2211.SetText(ind2.ToString());

                //    XWPFRun run2212 = para2212.CreateRun();
                //    para2212.Alignment = ParagraphAlignment.CENTER;
                //    run2212.FontFamily = "Times New Roman";
                //    run2212.FontSize = 12;
                //    run2212.SetText(item2.Employee.FirstName + "." + item2.Employee.LastName.Substring(0, 1) + "." + item2.Employee.MiddleName.Substring(0, 1));

                //    XWPFRun run2213 = para2213.CreateRun();
                //    para2213.Alignment = ParagraphAlignment.CENTER;
                //    run2213.FontFamily = "Times New Roman";
                //    run2213.FontSize = 12;
                //    run2213.SetText(item2.Sanatorium.Name + ", " + item2.Sanatorium.City.Name);

                //    XWPFRun run2214 = para2214.CreateRun();
                //    para2214.Alignment = ParagraphAlignment.CENTER;
                //    run2214.FontFamily = "Times New Roman";
                //    run2214.FontSize = 12;
                //    run2214.SetText(Convert.ToDateTime(item2.S).ToString("d"));

                //    XWPFRun run2215 = para2215.CreateRun();
                //    para2215.Alignment = ParagraphAlignment.CENTER;
                //    run2215.FontFamily = "Times New Roman";
                //    run2215.FontSize = 12;
                //    run2215.SetText(Convert.ToDateTime(item2.Po).ToString("d"));

                //    XWPFRun run2216 = para2216.CreateRun();
                //    para2216.Alignment = ParagraphAlignment.CENTER;
                //    run2216.FontFamily = "Times New Roman";
                //    run2216.FontSize = 12;
                //    run2216.SetText(g.ToString());

                //}
                //table221.SetColumnWidth(0, 100);
                //table221.SetColumnWidth(1, 1400);
                //table221.SetColumnWidth(2, 1500);
                //table221.SetColumnWidth(3, 1000);
                //table221.SetColumnWidth(4, 1000);
                //table221.SetColumnWidth(5, 1000);
                //-------------------------------низ что надо раскоментировать--------------------------------------------------------------------------

                
                int qind2 = 0;
                int str = 0;

                //-------Найдем количество строк в таблице. Без foreach не получается---------------------------
                int kolstr = 0;
                foreach(var i in GroupList)
                {
                    kolstr++;
                    foreach(var t in i.child)
                    {
                        kolstr++;
                    }
                }
                //----------------------------------------------------------------------------------------------

                XWPFTable qtable221 = doc.CreateTable(kolstr + 1, 7);
                qtable221.Width = 5000;

                //--Заполняю шапку таблицы----------------------------------------------------

                XWPFParagraph qpara22110 = qtable221.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph qpara22120 = qtable221.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFParagraph qpara22130 = qtable221.GetRow(0).GetCell(2).Paragraphs[0];
                XWPFParagraph qpara22140 = qtable221.GetRow(0).GetCell(3).Paragraphs[0];
                XWPFParagraph qpara22150 = qtable221.GetRow(0).GetCell(4).Paragraphs[0];
                XWPFParagraph qpara22160 = qtable221.GetRow(0).GetCell(5).Paragraphs[0];
                XWPFParagraph qpara22170 = qtable221.GetRow(0).GetCell(6).Paragraphs[0];

                XWPFRun qrun22110 = qpara22110.CreateRun();
                qpara22110.Alignment = ParagraphAlignment.CENTER;
                qrun22110.FontFamily = "Times New Roman";
                qrun22110.FontSize = 12;
                qrun22110.SetText("№ п/п");

                XWPFRun qrun22120 = qpara22120.CreateRun();
                qpara22120.Alignment = ParagraphAlignment.CENTER;
                qrun22120.FontFamily = "Times New Roman";
                qrun22120.FontSize = 12;
                qrun22120.SetText("ФИО работника/ пенсионера");

                XWPFRun qrun22130 = qpara22130.CreateRun();
                qpara22130.Alignment = ParagraphAlignment.CENTER;
                qrun22130.FontFamily = "Times New Roman";
                qrun22130.FontSize = 12;
                qrun22130.SetText("Место оздоровления");

                XWPFRun qrun22140 = qpara22140.CreateRun();
                qpara22140.Alignment = ParagraphAlignment.CENTER;
                qrun22140.FontFamily = "Times New Roman";
                qrun22140.FontSize = 12;
                qrun22140.SetText("Дата С");

                XWPFRun qrun22150 = qpara22150.CreateRun();
                qpara22150.Alignment = ParagraphAlignment.CENTER;
                qrun22150.FontFamily = "Times New Roman";
                qrun22150.FontSize = 12;
                qrun22150.SetText("Дата По");

                XWPFRun qrun22160 = qpara22160.CreateRun();
                qpara22160.Alignment = ParagraphAlignment.CENTER;
                qrun22160.FontFamily = "Times New Roman";
                qrun22160.FontSize = 12;
                qrun22160.SetText("Кол-во дней");
                
                XWPFRun qrun22170 = qpara22170.CreateRun();
                qpara22170.Alignment = ParagraphAlignment.CENTER;
                qrun22170.FontFamily = "Times New Roman";
                qrun22170.FontSize = 12;
                qrun22170.SetText("На кого");

                foreach (var qitem2 in GroupList)
                {                    
                     qind2++;
                    if(qitem2.who != "детей")
                    {
                        str++;
                    }
                                        
                    double qqg = (Convert.ToDateTime(qitem2.po) - Convert.ToDateTime(qitem2.s)).TotalDays + 1;

                    XWPFParagraph qpara2211 = qtable221.GetRow(qind2).GetCell(0).Paragraphs[0];
                    XWPFParagraph qpara2212 = qtable221.GetRow(qind2).GetCell(1).Paragraphs[0];
                    XWPFParagraph qpara2213 = qtable221.GetRow(qind2).GetCell(2).Paragraphs[0];
                    XWPFParagraph qpara2214 = qtable221.GetRow(qind2).GetCell(3).Paragraphs[0];
                    XWPFParagraph qpara2215 = qtable221.GetRow(qind2).GetCell(4).Paragraphs[0];
                    XWPFParagraph qpara2216 = qtable221.GetRow(qind2).GetCell(5).Paragraphs[0];
                    XWPFParagraph qpara2217 = qtable221.GetRow(qind2).GetCell(6).Paragraphs[0];

                    XWPFRun qrun2211 = qpara2211.CreateRun();
                    qpara2211.Alignment = ParagraphAlignment.CENTER;
                    qrun2211.FontFamily = "Times New Roman";
                    qrun2211.FontSize = 12;
                    if (qitem2.who != "детей")
                    {
                        qrun2211.SetText(str.ToString());
                    }
                    else
                    {
                        qrun2211.SetText("");
                    }

                    XWPFRun qrun2212 = qpara2212.CreateRun();
                    qpara2212.Alignment = ParagraphAlignment.CENTER;
                    qrun2212.FontFamily = "Times New Roman";
                    qrun2212.FontSize = 12;
                    qrun2212.SetText(qitem2.employee);
                                        
                    XWPFRun qrun2213 = qpara2213.CreateRun();
                    qpara2213.Alignment = ParagraphAlignment.CENTER;
                    qrun2213.FontFamily = "Times New Roman";
                    qrun2213.FontSize = 12;
                    qrun2213.SetText(qitem2.sanatorium + ", " + qitem2.city);

                    XWPFRun qrun2214 = qpara2214.CreateRun();
                    qpara2214.Alignment = ParagraphAlignment.CENTER;
                    qrun2214.FontFamily = "Times New Roman";
                    qrun2214.FontSize = 12;
                    qrun2214.SetText(Convert.ToDateTime(qitem2.s).ToString("d"));

                    XWPFRun qrun2215 = qpara2215.CreateRun();
                    qpara2215.Alignment = ParagraphAlignment.CENTER;
                    qrun2215.FontFamily = "Times New Roman";
                    qrun2215.FontSize = 12;
                    qrun2215.SetText(Convert.ToDateTime(qitem2.po).ToString("d"));

                    XWPFRun qrun2216 = qpara2216.CreateRun();
                    qpara2216.Alignment = ParagraphAlignment.CENTER;
                    qrun2216.FontFamily = "Times New Roman";
                    qrun2216.FontSize = 12;
                    qrun2216.SetText(qqg.ToString());

                    XWPFRun qrun2217 = qpara2217.CreateRun();
                    qpara2217.Alignment = ParagraphAlignment.CENTER;
                    qrun2217.FontFamily = "Times New Roman";
                    qrun2217.FontSize = 12;
                    qrun2217.SetText(qitem2.who);

                    foreach (var it in qitem2.child)
                    {
                        qind2++;
                        str++;

                        XWPFParagraph qqpara2211 = qtable221.GetRow(qind2).GetCell(0).Paragraphs[0];                        
                        XWPFTableRow row00 = qtable221.GetRow(qind2);
                        row00.MergeCells(2, 6);
                        XWPFParagraph qqpara2212 = qtable221.GetRow(qind2).GetCell(1).Paragraphs[0];
                        XWPFParagraph qqpara2213 = qtable221.GetRow(qind2).GetCell(2).Paragraphs[0];
                                                                                                
                        XWPFRun qqrun2211 = qqpara2211.CreateRun();
                        qqpara2211.Alignment = ParagraphAlignment.CENTER;
                        qqrun2211.FontFamily = "Times New Roman";
                        qqrun2211.FontSize = 12;
                        qqrun2211.SetText(str.ToString());

                        XWPFRun qqrun2212 = qqpara2212.CreateRun();
                        qqpara2212.Alignment = ParagraphAlignment.LEFT;
                        qqrun2212.FontFamily = "Times New Roman";
                        qqrun2212.FontSize = 12;
                        qqrun2212.SetText("");

                        XWPFRun qqrun2213 = qqpara2213.CreateRun();
                        qqpara2213.Alignment = ParagraphAlignment.LEFT;
                        qqrun2213.FontFamily = "Times New Roman";
                        qqrun2213.FontSize = 12;
                        qqrun2213.SetText(it.Fio + ", " + Convert.ToDateTime(it.DateBirth).ToString("d") + " г.р.");

                    }

                }
                qtable221.SetColumnWidth(0, 100);
                qtable221.SetColumnWidth(1, 1300);
                qtable221.SetColumnWidth(2, 1300);
                qtable221.SetColumnWidth(3, 575);
                qtable221.SetColumnWidth(4, 575);
                qtable221.SetColumnWidth(5, 575);
                qtable221.SetColumnWidth(5, 575);

                //-------Выводим подпись секретаря комиссии---------------------------------

                var p1111 = doc.CreateParagraph();
                p1111.Alignment = ParagraphAlignment.LEFT;
                //p111.IndentationFirstLine = 500;
                XWPFRun r1111 = p1111.CreateRun();
                r1111.FontFamily = "Times New Roman";
                r1111.FontSize = 12;
                r1111.IsBold = false;
                r1111.SetText("");

                XWPFTable table55 = doc.CreateTable(1, 3);

                table55.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table55.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table55.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table55.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table55.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table55.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para551 = table55.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para552 = table55.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFParagraph para553 = table55.GetRow(0).GetCell(2).Paragraphs[0];
                XWPFRun run551 = para551.CreateRun();
                XWPFRun run552 = para552.CreateRun();
                XWPFRun run553 = para553.CreateRun();

                run551.FontFamily = "Times New Roman";
                run551.FontSize = 12;
                if (secretar != null)
                {
                    run551.SetText(secretar.Status.Name);
                }
                else
                {
                    run551.SetText("");
                }

                run552.FontFamily = "Times New Roman";
                run552.FontSize = 12;
                run552.SetText("\t\t\t" + "______________________");

                run553.FontFamily = "Times New Roman";
                run553.FontSize = 12;

                if (secretar != null)
                {
                    run553.SetText("\t\t\t\t" + secretar.Employee.LastName.Substring(0, 1) + "." + secretar.Employee.MiddleName.Substring(0, 1) + "." + secretar.Employee.FirstName);
                }
                else
                {
                    run553.SetText("");
                }

                table55.SetColumnWidth(0, 2000);
                table55.SetColumnWidth(1, 2000);
                table55.SetColumnWidth(2, 2000);

                var pend = doc.CreateParagraph();
                pend.Alignment = ParagraphAlignment.LEFT;
                //p111.IndentationFirstLine = 500;
                XWPFRun rend = pend.CreateRun();
                rend.FontFamily = "Times New Roman";
                rend.FontSize = 12;
                rend.IsBold = false;
                rend.SetText("");
                //--------------------------------------------------------------------------

                doc.Write(fs);
                //fs.Close();
            }

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "C:/Install/Protocol.docx");
            FileStream fs1 = new FileStream(path, FileMode.Open);
            string file_type = "text/plain";
            string file_name = "Протокол №_" + pr.NumberP + "_от_" + Convert.ToDateTime(pr.DateProt).ToString("d") + ".docx";
            return File(fs1, file_type, file_name);

        }

        //------------------------------Печать заявления---------------------------------------------
        //----------------------------------------------------------------------------------
        public ActionResult Report(int ID)
        {
            Zayavlenie zay = new Zayavlenie();
            zay = db.Zayavlenie.Include(t => t.Employee.Filial).Include(o => o.Employee.Position).Include(ee => ee.Employee.Department).Include(p => p.TableZay).Include(yu => yu.Protocol).Include(e => e.Sanatorium).FirstOrDefault(g => g.Id == ID);

            List<Komissiya> listkom = new List<Komissiya>();
            listkom = db.Komissiya.Include(h => h.Status).Include(w => w.Employee.Position).Include(e => e.Employee.Department).OrderBy(u => u.Status.Priznak).Where(t => t.Employee.FilialId == Convert.ToInt32(zay.Employee.FilialId)).ToList();

            Komissiya predsed = listkom.FirstOrDefault(j => j.Status.Name.Trim() == "Председатель");

            TimeSpan? DaysRest = zay.Po - zay.S;

            //----Выводим список родственников(детей) согласно заявления---------------------------
            List<TableZay> listTabZay = new List<TableZay>();
            listTabZay = db.TableZay.Include(y => y.Child).Where(qq => qq.ZayId == ID).ToList();
            //-------------------------------------------------------------------------------------

            var newFile2 = @"C:/Install/Zayavlenie.docx";
            //var newFile2 = @"wwwroot/Report/Zayavlenie.docx";
            using (var fs = new FileStream(newFile2, FileMode.Create, FileAccess.Write))
            {
                XWPFDocument doc = new XWPFDocument();

                //----Заполняем шапку заявления-----------------------------------------------------------------------------------------------------------------------------------------------------

                XWPFTable table1 = doc.CreateTable(2, 2);
                table1.Width = 5000;

                table1.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para1 = table1.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para2 = table1.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFParagraph para3 = table1.GetRow(1).GetCell(0).Paragraphs[0];
                XWPFParagraph para4 = table1.GetRow(1).GetCell(1).Paragraphs[0];

                XWPFRun run1 = para1.CreateRun();
                run1.FontFamily = "Times New Roman";
                run1.FontSize = 10;
                run1.SetText("\t\t\t\t\t\t\t\t\t\t\t\t");

                XWPFRun run2 = para2.CreateRun();
                run2.FontFamily = "Times New Roman";
                run2.FontSize = 10;
                run2.SetText("Председателю комиссии по оздоровлению и");

                para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2R = para2.CreateRun();
                run2R.FontFamily = "Times New Roman";
                run2R.FontSize = 10;
                run2R.SetText("санаторно-курортному лечению " + zay.Employee.Filial.Name);

                //para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                //XWPFRun run2RR = para2.CreateRun();
                //run2RR.FontFamily = "Times New Roman";
                //run2RR.FontSize = 10;
                //run2RR.SetText(zay.Employee.Filial.Name);

                para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2RRR = para2.CreateRun();
                run2RRR.FontFamily = "Times New Roman";
                run2RRR.FontSize = 10;
                run2RRR.SetText("ОАО \"Гомельтранснефть Дружба\"");

                para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2RRRR = para2.CreateRun();
                run2RRRR.FontFamily = "Times New Roman";
                run2RRRR.FontSize = 10;
                if (predsed.Employee.Fiodp == null || predsed.Employee.Fiodp == "")
                {
                    run2RRRR.SetText(predsed.Employee.FirstName + " " + predsed.Employee.LastName.Substring(0, 1) + "." + predsed.Employee.MiddleName.Substring(0, 1) + ".");
                }
                else
                {
                    run2RRRR.SetText(predsed.Employee.Fiodp);
                }

                para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2RRRRR = para2.CreateRun();
                run2RRRRR.FontFamily = "Times New Roman";
                run2RRRRR.FontSize = 10;
                run2RRRRR.SetText(zay.Employee.Position.Name);

                para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2RRRRRR = para2.CreateRun();
                run2RRRRRR.FontFamily = "Times New Roman";
                run2RRRRRR.FontSize = 10;
                run2RRRRRR.SetText(zay.Employee.Department.Name);

                para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2RRRRRRR = para2.CreateRun();
                run2RRRRRRR.FontFamily = "Times New Roman";
                run2RRRRRRR.FontSize = 10;
                if (zay.Employee.Fiorp == null || zay.Employee.Fiorp == "")
                {
                    run2RRRRRRR.SetText(zay.Employee.FirstName + " " + zay.Employee.LastName.Substring(0, 1) + "." + zay.Employee.MiddleName.Substring(0, 1) + ". ");
                }
                else
                {
                    run2RRRRRRR.SetText(zay.Employee.Fiorp);
                }


                XWPFRun run3 = para3.CreateRun();
                run3.FontFamily = "Times New Roman";
                run3.FontSize = 10;
                run3.SetText("ЗАЯВЛЕНИЕ");

                para3.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run3R = para3.CreateRun();
                run3R.FontFamily = "Times New Roman";
                run3R.FontSize = 10;
                run3R.SetText(Convert.ToDateTime(zay.DateZ).ToString("d") + " № " +zay.NumberZ);

                para3.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run3RR = para3.CreateRun();
                run3RR.FontFamily = "Times New Roman";
                run3RR.FontSize = 10;
                if (zay.Employee.Filial.Priznak != null)
                {
                    run3RR.SetText(zay.Employee.Filial.Priznak);
                }
                else
                {
                    run3RR.SetText("");
                }

                table1.SetColumnWidth(0, 2000);
                table1.SetColumnWidth(1, 3000);

                //---Вставляем пустой параграф---------------------------------------------
                //var p110 = doc.CreateParagraph();
                //p110.Alignment = ParagraphAlignment.LEFT;
                ////p111.IndentationFirstLine = 500;
                //XWPFRun r110 = p110.CreateRun();
                //r110.FontFamily = "Times New Roman";
                //r110.FontSize = 10;
                //r110.IsBold = false;
                //r110.SetText("");

                var qp1 = doc.CreateParagraph();
                qp1.Alignment = ParagraphAlignment.BOTH;
                qp1.IndentationFirstLine = 500;
                XWPFRun qr1 = qp1.CreateRun();
                qr1.FontFamily = "Times New Roman";
                qr1.FontSize = 10;
                qr1.IsBold = false;
                qr1.SetText("В соответствии с Разделом 7 действующего Коллективного договора ОАО \"Гомельтранснефть Дружба\", прошу рассмотреть вопрос о выделении денежных средств с последующей оплатой на санаторно-курортное лечение и оздоровление в " + zay.Sanatorium.Name);

                //qp1.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                //XWPFRun qp2 = qp1.CreateRun();
                //qp2.FontFamily = "Times New Roman";
                //qp2.FontSize = 10;
                //qp2.SetText(zay.Sanatorium.Name);

                qp1.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qp22 = qp1.CreateRun();
                qp22.FontFamily = "Times New Roman";
                qp22.FontSize = 10;
                qp22.SetText("в период с " + Convert.ToDateTime(zay.S).ToString("d") + " по " + Convert.ToDateTime(zay.Po).ToString("d") + " продолжительностью " + ((Convert.ToDateTime(zay.Po) - Convert.ToDateTime(zay.S)).TotalDays + 1) + " дней");

                //-----Заполняем общий текст в параграфе------------------------------------------------------------------------
                var p1 = doc.CreateParagraph();
                p1.Alignment = ParagraphAlignment.BOTH;
                p1.IndentationFirstLine = 500;
                XWPFRun r1 = p1.CreateRun();
                r1.FontFamily = "Times New Roman";
                r1.FontSize = 10;
                r1.IsBold = false;
                r1.SetText("Мне известны положения действующего Коллективного договора о порядке отчета по использованию путевок, оплаченных за счёт средств организации.");

                p1.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qqp2 = p1.CreateRun();
                qqp2.FontFamily = "Times New Roman";
                qqp2.FontSize = 10;
                qqp2.SetText("Работник, пенсионер с даты возвращения (после выхода из отпуска) в трехдневный срок (рабочие дни) обязан предоставить в комиссию по оздоровлению филиала " + zay.Employee.Filial.Name + ":");

                var p2 = doc.CreateParagraph();
                p2.Alignment = ParagraphAlignment.BOTH;
                //p2.IndentationFirstLine = 500;
                XWPFRun r2 = p2.CreateRun();
                r2.FontFamily = "Times New Roman";
                r2.FontSize = 10;
                r2.IsBold = false;
                r2.SetText("- документы (договор, ваучер, отрывной талон от путевки и иные документы, подтверждающие проживание, получение санаторно-курортного лечения и (или) оздоровления, проезд до места оказания услуг, копию страниц паспорта с отметкой о пересечении границы при выезде на санаторно-курортное лечение или оздоровление за пределы Республики Беларусь, кроме Российской Федерации;)");

                var p3 = doc.CreateParagraph();
                p3.Alignment = ParagraphAlignment.BOTH;
                //p1.IndentationFirstLine = 500;
                XWPFRun r3 = p3.CreateRun();
                r3.FontFamily = "Times New Roman";
                r3.FontSize = 10;
                r3.IsBold = false;
                r3.SetText("- в случае непредоставления документов, указанных выше, а также при нарушении порядка использования путевки (средств выделяемых ОАО на оздоровление), работник (пенсионер) лишается права на получение частично или полностью оплачиваемой ОАО путевки (услуг по оздоровлению) на себя и на ребенка в течение 3-х лет с момента обнаружения допущенного нарушения. При этом работник (пенсионер) обязан возместить ОАО перечисленную за него и (или) его детей сумму на приобретение предоставленной ему и (или) его детям путевки (услуг по оздоровлению);");

                var p33 = doc.CreateParagraph();
                p33.Alignment = ParagraphAlignment.BOTH;
                //p1.IndentationFirstLine = 500;
                XWPFRun r33 = p33.CreateRun();
                r33.FontFamily = "Times New Roman";
                r33.FontSize = 10;
                r33.IsBold = false;
                r33.SetText("- в случае моего отказа от заявленного мною оздоровления без уважительных причин, обязуюсь уплатить полную стоимость, оплаченную мне организацией на оздоровление и даю согласие бухгалтерии на удержание этой суммы из моей заработной платы и других причитающихся выплат.");


                var p4 = doc.CreateParagraph();
                p4.Alignment = ParagraphAlignment.BOTH;
                XWPFRun r4 = p4.CreateRun();
                r4.FontFamily = "Times New Roman";
                r4.FontSize = 10;
                r4.IsBold = false;
                r4.SetText("");

                //----Вставляем таблицу с подписью работника------------------------------------------------------------------
                XWPFTable table3 = doc.CreateTable(2, 3);
                table3.Width = 5000;

                table3.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                //table3.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para31 = table3.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para32 = table3.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFParagraph para33 = table3.GetRow(0).GetCell(2).Paragraphs[0];
                XWPFParagraph para34 = table3.GetRow(1).GetCell(0).Paragraphs[0];
                XWPFParagraph para35 = table3.GetRow(1).GetCell(1).Paragraphs[0];
                XWPFParagraph para36 = table3.GetRow(1).GetCell(2).Paragraphs[0];

                XWPFRun run31 = para31.CreateRun();
                run31.FontFamily = "Times New Roman";
                run31.FontSize = 10;
                run31.SetText(zay.Employee.Position.Name);

                XWPFRun run32 = para32.CreateRun();
                run32.FontFamily = "Times New Roman";
                run32.FontSize = 10;
                run32.SetText("\t\t____________________");

                XWPFRun run33 = para33.CreateRun();
                run33.FontFamily = "Times New Roman";
                run33.FontSize = 10;
                run33.SetText("\t\t" + zay.Employee.FirstName + " " + zay.Employee.LastName.Substring(0, 1) + "." + zay.Employee.MiddleName.Substring(0, 1) + ".");

                XWPFRun run34 = para34.CreateRun();
                run34.FontFamily = "Times New Roman";
                run34.FontSize = 10;
                run34.SetText("");

                XWPFRun run35 = para35.CreateRun();
                run35.FontFamily = "Times New Roman";
                run35.FontSize = 10;
                run35.SetText("\t\t\t(подпись)");

                XWPFRun run36 = para36.CreateRun();
                run36.FontFamily = "Times New Roman";
                run36.FontSize = 10;
                run36.SetText("");

                //-----Заполняем выписку из протокола (то, что под чертой)---------------------------

                var p5 = doc.CreateParagraph();
                p5.Alignment = ParagraphAlignment.CENTER;
                XWPFRun r5 = p5.CreateRun();
                r5.FontFamily = "Times New Roman";
                r5.FontSize = 10;
                r5.IsBold = false;
                r5.SetText("ВЫПИСКА");

                var p6 = doc.CreateParagraph();
                p6.Alignment = ParagraphAlignment.LEFT;
                XWPFRun r6 = p6.CreateRun();
                r6.FontFamily = "Times New Roman";
                r6.FontSize = 10;
                r6.IsBold = false;
                r6.SetText("из протокола № " + zay.Protocol.NumberP + " от " + Convert.ToDateTime(zay.Protocol.DateProt).ToString("d") + " заседания комиссии по оздоровлению и санаторно-курортному лечению " + zay.Employee.Filial.Name + " ОАО \"Гомельтранснефть Дружба\".");

                string fiorp;
                if (zay.Employee.Fiorp == null || zay.Employee.Fiorp == "")
                {
                    fiorp = zay.Employee.FirstName + " " + zay.Employee.LastName.Substring(0, 1) + "." + zay.Employee.MiddleName.Substring(0, 1) + ".";
                }
                else
                {
                    fiorp = zay.Employee.Fiorp;
                }

                //--Определим на кого выписано заявление и выведем в соответствующих падежах работника или пенсионера--------------------
                string whoDP = "";
                string whoRp = "";
                if (zay.Who == "пенсионера")
                {
                    whoDP = "пенсионеру";
                    whoRp = "пенсионера";
                }
                else
                {
                    whoDP = "работнику";
                    whoRp = "работника";
                }


                p6.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun r6R = p6.CreateRun();
                r6R.FontFamily = "Times New Roman";
                r6R.FontSize = 10;
                r6R.IsBold = false;
                r6R.SetText("СЛУШАЛИ: председателя комиссии по оздоровлению и санаторно-курортному лечению " + zay.Employee.Filial.Name + " по вопросу рассмотрения заявления " + whoRp + " " + fiorp + " о выделении денежных средств на санаторно-курортное лечение и оздоровление в соответствии с Разделом 7 действующего Коллективного договора ОАО.");

                p6.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr6RR = p6.CreateRun();
                qr6RR.FontFamily = "Times New Roman";
                qr6RR.FontSize = 10;
                qr6RR.IsBold = false;
                qr6RR.SetText("Месяц: " + Convert.ToDateTime(zay.S).Month + " " + Convert.ToDateTime(zay.S).Year + " года, санаторий: " + zay.Sanatorium.Name);

                string fiodp;
                if (zay.Employee.Fiodp == null || zay.Employee.Fiodp == "")
                {
                    fiodp = zay.Employee.FirstName + " " + zay.Employee.LastName.Substring(0, 1) + "." + zay.Employee.MiddleName.Substring(0, 1) + ".";
                }
                else
                {
                    fiodp = zay.Employee.Fiodp;
                }

                //--Определяем на кого выписано заявлене и если на детей или семейная, то выведем список отдыхающих----------------------

                if (zay.Who == "работника" || zay.Who == "пенсионера")
                {
                    p6.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun r6RR = p6.CreateRun();
                    r6RR.FontFamily = "Times New Roman";
                    r6RR.FontSize = 10;
                    r6RR.IsBold = false;
                    r6RR.SetText("ПОСТАНОВИЛИ: Выделить денежные средства с последующей оплатой на санаторно-курортное лечение и оздоровление в соответствии с Разделом 7 действующего Коллективного договора ОАО " + "в период с " + Convert.ToDateTime(zay.S).ToString("d") + " по " + Convert.ToDateTime(zay.Po).ToString("d") + " продолжительностью " + ((Convert.ToDateTime(zay.Po) - Convert.ToDateTime(zay.S)).TotalDays + 1)  + " дней в " + zay.Sanatorium.Name + " " + whoDP + " " + zay.Employee.Filial.Name + " " + fiodp);

                }
                else if (zay.Who == "детей")
                {
                    p6.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun r6RR = p6.CreateRun();
                    r6RR.FontFamily = "Times New Roman";
                    r6RR.FontSize = 10;
                    r6RR.IsBold = false;
                    r6RR.SetText("ПОСТАНОВИЛИ: Выделить денежные средства с последующей оплатой на санаторно-курортное лечение и оздоровление в соответствии с Разделом 7 действующего Коллективного договора ОАО " + "в период с " + Convert.ToDateTime(zay.S).ToString("d") + " по " + Convert.ToDateTime(zay.Po).ToString("d") + " продолжительностью " + ((Convert.ToDateTime(zay.Po) - Convert.ToDateTime(zay.S)).TotalDays +1)+ " дней в " + zay.Sanatorium.Name + " детям " + whoRp + " " + zay.Employee.Filial.Name + " " + fiodp);

                    int ind = -1;
                    XWPFTable table2 = doc.CreateTable(listTabZay.Count, 2);
                    table2.Width = 5000;

                    table2.SetColumnWidth(0, 2500);
                    table2.SetColumnWidth(1, 2500);

                    table2.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table2.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table2.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table2.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table2.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table2.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                    foreach (var item1 in listTabZay)
                    {
                        ind++;
                        XWPFParagraph para21 = table2.GetRow(ind).GetCell(0).Paragraphs[0];
                        XWPFParagraph para22 = table2.GetRow(ind).GetCell(1).Paragraphs[0];

                        XWPFRun run21 = para21.CreateRun();
                        run21.FontFamily = "Times New Roman";
                        run21.FontSize = 10;
                        run21.SetText("\t" + item1.Child.Fio);

                        XWPFRun run22 = para22.CreateRun();
                        run22.FontFamily = "Times New Roman";
                        run22.FontSize = 10;
                        run22.SetText(Convert.ToDateTime(item1.Child.DateBirth).ToString("d") + " года рождения");
                    }
                }
                else if (zay.Who == "семейная")
                {
                    p6.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun r6RR = p6.CreateRun();
                    r6RR.FontFamily = "Times New Roman";
                    r6RR.FontSize = 10;
                    r6RR.IsBold = false;
                    r6RR.SetText("ПОСТАНОВИЛИ: Выделить денежные средства с последующей оплатой на санаторно-курортное лечение и оздоровление в соответствии с Разделом 7 действующего Коллективного договора ОАО " + "в период с " + Convert.ToDateTime(zay.S).ToString("d") + " по " + Convert.ToDateTime(zay.Po).ToString("d") + " продолжительностью " + (Convert.ToDateTime(zay.Po) - Convert.ToDateTime(zay.S)).TotalDays + " дней в " + zay.Sanatorium.Name + " " + whoDP + " " + zay.Employee.Filial.Name + " " + fiodp + " и его детям:");

                    int ind = -1;
                    XWPFTable table2 = doc.CreateTable(listTabZay.Count, 2);
                    table2.Width = 5000;

                    table2.SetColumnWidth(0, 2500);
                    table2.SetColumnWidth(1, 2500);

                    table2.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table2.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table2.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table2.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table2.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table2.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                    foreach (var item1 in listTabZay)
                    {
                        ind++;
                        XWPFParagraph para21 = table2.GetRow(ind).GetCell(0).Paragraphs[0];
                        XWPFParagraph para22 = table2.GetRow(ind).GetCell(1).Paragraphs[0];

                        XWPFRun run21 = para21.CreateRun();
                        run21.FontFamily = "Times New Roman";
                        run21.FontSize = 10;
                        run21.SetText("\t" + item1.Child.Fio);

                        XWPFRun run22 = para22.CreateRun();
                        run22.FontFamily = "Times New Roman";
                        run22.FontSize = 10;
                        run22.SetText(Convert.ToDateTime(item1.Child.DateBirth).ToString("d") + " года рождения");
                    }
                    //var qp11 = doc.CreateParagraph();
                    //qp11.Alignment = ParagraphAlignment.BOTH;
                    //XWPFRun qr11 = qp11.CreateRun();
                    //qr11.FontFamily = "Times New Roman";
                    //qr11.FontSize = 12;
                    //qr11.IsBold = false;
                    //qr11.SetText("в период с " + Convert.ToDateTime(zay.S).ToString("d") + " по " + Convert.ToDateTime(zay.Po).ToString("d") + " продолжительностью " + Convert.ToDateTime(zay.Po).Subtract(Convert.ToDateTime(zay.S)).Days + " дней.");
                }
                //-----------------------------------------------------------------------------

                //p6.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                //XWPFRun r6RR = p6.CreateRun();
                //r6RR.FontFamily = "Times New Roman";
                //r6RR.FontSize = 10;
                //r6RR.IsBold = false;
                //r6RR.SetText("ПОСТАНОВИЛИ: Выделить денежные средства с последующей оплатой на санаторно-курортное лечение и оздоровление в соответствии с Разделом 7 действующего Коллективного договора ОАО " + "в период с " + Convert.ToDateTime(zay.S).ToString("d") + " по " + Convert.ToDateTime(zay.Po).ToString("d") + "продолжительностью " + (Convert.ToDateTime(zay.Po) - Convert.ToDateTime(zay.S)).TotalDays + " дней в " + zay.Sanatorium.Name + " " + whoDP + " " + zay.Employee.Filial.Name + " " + fiodp);

                //----Выводим подпись председателя комиссии--------------------------------------------------

                if (listkom.FirstOrDefault(e => e.Priznak == "1") != null)
                {
                    XWPFTable table5 = doc.CreateTable(1, 3);
                    table5.Width = 5000;

                    table5.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table5.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table5.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table5.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table5.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table5.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                    XWPFParagraph para51 = table5.GetRow(0).GetCell(0).Paragraphs[0];
                    XWPFParagraph para52 = table5.GetRow(0).GetCell(1).Paragraphs[0];
                    XWPFParagraph para53 = table5.GetRow(0).GetCell(2).Paragraphs[0];

                    XWPFRun run51 = para51.CreateRun();
                    run51.FontFamily = "Times New Roman";
                    run51.FontSize = 10;
                    run51.SetText(listkom.FirstOrDefault(e => e.Priznak == "1").Status.Name);

                    XWPFRun run52 = para52.CreateRun();
                    run52.FontFamily = "Times New Roman";
                    run52.FontSize = 10;
                    run52.SetText("\t__________________");

                    XWPFRun run53 = para53.CreateRun();
                    run53.FontFamily = "Times New Roman";
                    run53.FontSize = 10;
                    run53.SetText("\t" + listkom.FirstOrDefault(e => e.Priznak == "1").Employee.FirstName + " " + listkom.FirstOrDefault(e => e.Priznak == "1").Employee.LastName.Substring(0, 1) + "." + listkom.FirstOrDefault(e => e.Priznak == "1").Employee.MiddleName.Substring(0, 1) + ".");
                }
                else
                {
                    XWPFTable table5 = doc.CreateTable(1, 3);
                    table5.Width = 5000;

                    table5.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table5.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table5.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table5.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table5.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                    table5.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                    XWPFParagraph para51 = table5.GetRow(0).GetCell(0).Paragraphs[0];
                    XWPFParagraph para52 = table5.GetRow(0).GetCell(1).Paragraphs[0];
                    XWPFParagraph para53 = table5.GetRow(0).GetCell(2).Paragraphs[0];

                    XWPFRun run51 = para51.CreateRun();
                    run51.FontFamily = "Times New Roman";
                    run51.FontSize = 10;
                    run51.SetText("Председатель комиссии");

                    para51.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run51R = para51.CreateRun();
                    run51R.FontFamily = "Times New Roman";
                    run51R.FontSize = 10;
                    run51R.SetText("по оздоровлению");

                    para51.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run51RR = para51.CreateRun();
                    run51RR.FontFamily = "Times New Roman";
                    run51RR.FontSize = 10;
                    run51RR.SetText(predsed.Employee.Filial.Name);

                    para51.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run51RRR = para51.CreateRun();
                    run51RRR.FontFamily = "Times New Roman";
                    run51RRR.FontSize = 10;
                    run51RRR.SetText("ОАО \"Гомельтранснефть Дружба\"");

                    XWPFRun run52 = para52.CreateRun();
                    run52.FontFamily = "Times New Roman";
                    run52.FontSize = 10;
                    run52.SetText("");

                    para52.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run52R = para52.CreateRun();
                    run52R.FontFamily = "Times New Roman";
                    run52R.FontSize = 10;
                    run52R.SetText("");

                    para52.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run52RR = para52.CreateRun();
                    run52RR.FontFamily = "Times New Roman";
                    run52RR.FontSize = 10;
                    run52RR.SetText("");

                    para52.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run52RRR = para52.CreateRun();
                    run52RRR.FontFamily = "Times New Roman";
                    run52RRR.FontSize = 10;
                    run52RRR.SetText("\t__________________");

                    XWPFRun run53 = para53.CreateRun();
                    run53.FontFamily = "Times New Roman";
                    run53.FontSize = 10;
                    run53.SetText("");

                    para53.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run53R = para53.CreateRun();
                    run53R.FontFamily = "Times New Roman";
                    run53R.FontSize = 10;
                    run53R.SetText("");

                    para53.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run53RR = para53.CreateRun();
                    run53RR.FontFamily = "Times New Roman";
                    run53RR.FontSize = 10;
                    run53RR.SetText("");

                    para53.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run53RRR = para53.CreateRun();
                    run53RRR.FontFamily = "Times New Roman";
                    run53RRR.FontSize = 10;
                    run53RRR.SetText("\t" + predsed.Employee.FirstName + " " + predsed.Employee.LastName.Substring(0, 1) + "." + predsed.Employee.MiddleName.Substring(0, 1) + ".");
                }

                //-------------------------------------------------------------------------------------------

                doc.Write(fs);
                //fs.Close();
            }

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "C:/Install/Zayavlenie.docx");
            FileStream fs1 = new FileStream(path, FileMode.OpenOrCreate);
            string file_type = "text/plain";
            string file_name = "Заявлениел №_" + zay.NumberZ + "_от_" + Convert.ToDateTime(zay.DateZ).ToString("d") + ".docx";

            return File(fs1, file_type, file_name);

        }
        //-------------------------------------------------------------------------------------------
        //------------------------------Печать докладной записки на оплату---------------------------------------------
        public ActionResult ReportOplata(int ID)
        {
            Zayavlenie zay = new Zayavlenie();
            zay = db.Zayavlenie.Include(t => t.Employee.Filial).Include(o => o.Employee.Position).Include(ee => ee.Employee.Department).Include(p => p.TableZay).Include(yu => yu.Protocol).Include(h => h.Sanatorium.City.Country).Include(yy => yy.Sanatorium.Bank).Include(u=>u.TurOpe.City.Country).Include(tt=>tt.TurOpe.Bank).FirstOrDefault(g => g.Id == ID);

            List<Komissiya> listkom = new List<Komissiya>();
            listkom = db.Komissiya.Include(h => h.Status).Include(w => w.Employee.Position).Include(e => e.Employee.Department).OrderBy(u => u.Status.Priznak).Where(t => t.Employee.FilialId == Convert.ToInt32(zay.Employee.FilialId)).ToList();

            Komissiya ruk = listkom.FirstOrDefault(j => j.Status.Name.Trim() == "Руководитель");

            TimeSpan? DaysRest = zay.Po - zay.S;

            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee.Department).Include(t => t.Employee.Position).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            //----Выводим список родственников(детей) согласно заявления---------------------------
            List<TableZay> listTabZay = new List<TableZay>();
            listTabZay = db.TableZay.Include(y => y.Child).Where(qq => qq.ZayId == ID).ToList();
            //-------------------------------------------------------------------------------------

            var newFile2 = @"C:/Install/ReportOplata.docx";
            //var newFile2 = @"wwwroot/Report/ReportOplata.docx";
            using (var fs = new FileStream(newFile2, FileMode.Create, FileAccess.Write))
            {
                XWPFDocument doc = new XWPFDocument();

                //----Заполняем шапку заявления-----------------------------------------------------------------------------------------------------------------------------------------------------

                XWPFTable table1 = doc.CreateTable(2, 2);
                table1.Width = 5000;

                table1.SetColumnWidth(0, 2500);
                table1.SetColumnWidth(1, 2500);

                table1.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para1 = table1.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para2 = table1.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFParagraph para3 = table1.GetRow(1).GetCell(0).Paragraphs[0];
                XWPFParagraph para4 = table1.GetRow(1).GetCell(1).Paragraphs[0];

                XWPFRun run1 = para1.CreateRun();
                run1.FontFamily = "Times New Roman";
                run1.FontSize = 14;
                run1.SetText(UsFil.Employee.Department.Name);

                //para1.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                //XWPFRun run1RR = para1.CreateRun();
                //run1RR.FontFamily = "Times New Roman";
                //run1RR.FontSize = 14;
                //run1RR.SetText("");

                //para1.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                //XWPFRun run1RRR = para1.CreateRun();
                //run1RRR.FontFamily = "Times New Roman";
                //run1RRR.FontSize = 14;
                //run1RRR.SetText("ДОКЛАДНАЯ ЗАПИСКА");

                XWPFRun run2 = para2.CreateRun();
                run2.FontFamily = "Times New Roman";
                run2.FontSize = 14;
                if (ruk != null)
                {
                    run2.SetText(ruk.Employee.Position.Name);
                }
                else
                {
                    run2.SetText("");
                }

                para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2R = para2.CreateRun();
                run2R.FontFamily = "Times New Roman";
                run2R.FontSize = 14;
                if (ruk != null)
                {
                    run2R.SetText(ruk.Employee.FirstName + " " + ruk.Employee.LastName.Substring(0, 1) + "." + ruk.Employee.MiddleName.Substring(0, 1) + ".");
                }
                else
                {
                    run2R.SetText("");
                }

                XWPFRun run3 = para3.CreateRun();
                run3.FontFamily = "Times New Roman";
                run3.FontSize = 14;
                run3.SetText("ДОКЛАДНАЯ ЗАПИСКА\t\t");

                para3.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run3R = para3.CreateRun();
                run3R.FontFamily = "Times New Roman";
                run3R.FontSize = 14;
                if (UsFil.Employee.Department.Priznak != null)
                {
                    run3R.SetText(DateTime.Now.ToString("d") + " № " + UsFil.Employee.Department.Priznak + "/");
                }
                else
                {
                    run3R.SetText(DateTime.Now.ToString("d") + " № " + "/");
                }

                para3.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run3RR = para3.CreateRun();
                run3RR.FontFamily = "Times New Roman";
                run3RR.FontSize = 14;
                if (zay.Employee.Filial.Priznak != null)
                {
                    run3RR.SetText(zay.Employee.Filial.Priznak);
                }
                else
                {
                    run3RR.SetText("");
                }

                XWPFRun run4 = para4.CreateRun();
                run4.FontFamily = "Times New Roman";
                run4.IsItalic = true;
                run4.FontSize = 16;
                run4.SetText("Бухгалтерии");

                para4.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run4R = para4.CreateRun();
                run4R.FontFamily = "Times New Roman";
                run4R.IsItalic = true;
                run4R.FontSize = 16;
                run4R.SetText("к оплате");

                para4.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run4RR = para4.CreateRun();
                run4RR.FontFamily = "Times New Roman";
                run4RR.FontSize = 14;
                run4RR.SetText("_______________________");

                para4.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run4RRR = para4.CreateRun();
                run4RRR.FontFamily = "Times New Roman";
                run4RRR.FontSize = 14;
                run4RRR.SetText("____.____.20__");

                table1.SetColumnWidth(0, 2500);
                table1.SetColumnWidth(1, 2500);

                var p110 = doc.CreateParagraph();
                p110.Alignment = ParagraphAlignment.LEFT;
                //p111.IndentationFirstLine = 500;
                XWPFRun r110 = p110.CreateRun();
                r110.FontFamily = "Times New Roman";
                r110.FontSize = 14;
                r110.IsBold = false;
                r110.SetText("Об оплате в белорусских рублях.");

                string dog = "";
                if(zay.TurOpeId != null)
                {
                    dog = "от " + Convert.ToDateTime(zay.DateDog).ToString("d") + " № " + zay.NumberDog;
                }
                else
                {
                Dogovor DogD = new Dogovor();
                DogD = db.Dogovor.FirstOrDefault(k => k.SanatoriumId == zay.SanatoriumId && k.PriznakClose != "Закрыт" && (k.TypeDogId == 1 || k.TypeDogId == 3) && k.FillialId == UsFil.Employee.FilialId);
                if (DogD != null)
                {
                    dog = "от " + Convert.ToDateTime(DogD.DateDog).ToString("d") + " № " + DogD.Number;
                }
                else
                {
                    dog = "";
                }
                }
               

                string sum = "";
                if (zay.Summa == null)
                {
                    sum = "";
                }
                else
                {
                    sum = zay.Summa.ToString() + " белорусских рублей, без НДС ";
                }


                //--Проверяем есть ли РП для сотрудника-----
                string sotr = "";
                if (zay.Employee.Fiorp == null || zay.Employee.Fiorp == "")
                {
                    sotr = zay.Employee.FirstName + " " + zay.Employee.LastName.Substring(0, 1) + "." + zay.Employee.MiddleName.Substring(0, 1) + ".";
                }
                else
                {
                    sotr = zay.Employee.Fiorp;
                }
                //------------------------------------------

                //------Проверяем на кого выписана путевка---------
                string who = "";
                if (zay.Who == "работника")
                {
                    who = "работника " + zay.Employee.Filial.Name + " " + sotr;
                }
                else if (zay.Who == "пенсионера")
                {
                    who = "пенсионера " + zay.Employee.Filial.Name + " " + sotr;
                }
                else if (zay.Who == "детей")
                {
                    string child = "";
                    foreach (var item in zay.TableZay)
                    {
                        child = child + " " + item.Child.Fio + " " + Convert.ToDateTime(item.Child.DateBirth).ToString("d") + ", ";
                    }
                    child = child.Remove(child.Length - 1);
                    if (zay.TableZay.Count() > 1)
                    {
                        who = "детей " + child + " работника " + sotr + ".";
                    }
                    else
                    {
                        who = "ребёнка " + child + " работника " + sotr + ".";
                    }
                }
                else
                {
                    string child = "";
                    foreach (var item in zay.TableZay)
                    {
                        child = child + " " + item.Child.Fio + " " + Convert.ToDateTime(item.Child.DateBirth).ToString("d") + " г.р., ";
                    }
                    string childRem = child.Remove(child.Length - 2);
                    child = child.Remove(child.Length - 1);
                    if (zay.TableZay.Count() > 1)
                    {
                        who = " работника " + sotr + " и его детей " + child + ".";
                    }
                    else
                    {
                        who = " работника " + sotr + " и его ребёнка " + child + ".";
                    }
                }
                //-------------------------------------------------
                //Вставляем параграф с суммой----------------------------------------------
                var qp1 = doc.CreateParagraph();
                qp1.Alignment = ParagraphAlignment.BOTH;
                qp1.IndentationFirstLine = 500;
                XWPFRun qr1 = qp1.CreateRun();
                qr1.FontFamily = "Times New Roman";
                qr1.FontSize = 14;
                qr1.IsBold = false;
                if (zay.TurOpeId != null)
                {
 qr1.SetText("В соответствии с договором на оказание санаторно-курортных услуг " + dog + ", заключенным с " + zay.TurOpe.Name + ", прошу Вас дать поручение бухгалтерии оплатить сумму " + sum + " (" + RusCurrency.Str(Convert.ToDouble(zay.Summa)) + ") за оздоровление и санаторно-курортное лечение " + who);
                }
                else
                {
 qr1.SetText("В соответствии с договором на оказание санаторно-курортных услуг " + dog + ", заключенным с " + zay.Sanatorium.Name + ", прошу Вас дать поручение бухгалтерии оплатить сумму " + sum + " (" + RusCurrency.Str(Convert.ToDouble(zay.Summa)) + ") за оздоровление и санаторно-курортное лечение " + who);
                }               
                
                //--------------------------------------------------------------

                //Вставляем параграф с условиями оплаты----------------------------------------------
                var qp2 = doc.CreateParagraph();
                //qp2.Alignment = ParagraphAlignment.BOTH;
                //qp2.IndentationFirstLine = 500;
                XWPFRun qr2 = qp2.CreateRun();
                qr2.FontFamily = "Times New Roman";
                qr2.FontSize = 14;
                qr2.IsBold = false;
                qr2.SetText("Условия оплаты: ");
                qr2.SetUnderline(UnderlinePatterns.Single);
                //--------------------------------------------------------------

                XWPFRun qr2R = qp2.CreateRun();
                qr2R.FontFamily = "Times New Roman";
                qr2R.FontSize = 14;
                qr2R.IsBold = false;
                qr2R.SetText("Предоплата.");

                qp2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr3 = qp2.CreateRun();
                qr3.FontFamily = "Times New Roman";
                qr3.FontSize = 14;
                qr3.IsBold = false;
                qr3.SetText("Основание для оплаты: ");
                qr3.SetUnderline(UnderlinePatterns.Single);
                //--------------------------------------------------------------

                XWPFRun qr3R = qp2.CreateRun();
                qr3R.FontFamily = "Times New Roman";
                qr3R.FontSize = 14;
                qr3R.IsBold = false;
                qr3R.SetText("Договор на оказание санаторно-курортных услуг " + dog + ", счет-фактура, заявление " + sotr + " № " + zay.NumberZ + " от " + Convert.ToDateTime(zay.DateZ).ToString("d"));
                //-----------------------------

                qp2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr4 = qp2.CreateRun();
                qr4.FontFamily = "Times New Roman";
                qr4.FontSize = 14;
                qr4.IsBold = false;
                qr4.SetText("Срок оплаты:");
                qr4.SetUnderline(UnderlinePatterns.Single);
                //--------------------------------------------------------------

                qp2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr5 = qp2.CreateRun();
                qr5.FontFamily = "Times New Roman";
                qr5.FontSize = 14;
                qr5.IsBold = false;
                qr5.SetText("Обоснование цены: ");
                qr5.SetUnderline(UnderlinePatterns.Single);
                //--------------------------------------------------------------

                XWPFRun qr5R = qp2.CreateRun();
                qr5R.FontFamily = "Times New Roman";
                qr5R.FontSize = 14;
                qr5R.IsBold = false;
                qr5R.SetText("Договор на оказание санаторно-курортных услуг " + dog + ", счет-фактура");
                //-------------------

                qp2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr55 = qp2.CreateRun();
                qr55.FontFamily = "Times New Roman";
                qr55.FontSize = 14;
                qr55.IsBold = false;
                qr55.SetText("Код назначения платежа: ");
                qr55.SetUnderline(UnderlinePatterns.Single);
                //--------------------------------------------------------------

                XWPFRun qr55R5 = qp2.CreateRun();
                qr55R5.FontFamily = "Times New Roman";
                qr55R5.FontSize = 14;
                qr55R5.IsBold = false;
                qr55R5.SetText("20701 ");
                //-------------------

                //-----Заполняем общий текст в параграфе------------------------------------------------------------------------
                var p1 = doc.CreateParagraph();
                p1.Alignment = ParagraphAlignment.BOTH;
                p1.IndentationFirstLine = 500;
                XWPFRun r1 = p1.CreateRun();
                r1.FontFamily = "Times New Roman";
                r1.FontSize = 14;
                r1.IsBold = false;
                if(zay.TurOpeId != null)
                {
               r1.SetText("Сведения о нахождении " + zay.TurOpe.Name + " в перечне организаций и индивидуальных предпринимателей, в отношении которых ДФР КГК Республики Беларусь составлено заключение об установлении оснований, указанных в пункте 4 статьи 33 Налогового кодекса Республики Беларусь, на официальном сайте Комитета государственного контроля Республики Беларусь www.kgk.gov.by/ru/perechen_lzhestruktur-ru/ по состоянию на " + DateTime.Now.ToString("d") + " отсутствуют.");
                }
                else
                {
               r1.SetText("Сведения о нахождении " + zay.Sanatorium.Name + " в перечне организаций и индивидуальных предпринимателей, в отношении которых ДФР КГК Республики Беларусь составлено заключение об установлении оснований, указанных в пункте 4 статьи 33 Налогового кодекса Республики Беларусь, на официальном сайте Комитета государственного контроля Республики Беларусь www.kgk.gov.by/ru/perechen_lzhestruktur-ru/ по состоянию на " + DateTime.Now.ToString("d") + " отсутствуют.");
                }
                

                var p2 = doc.CreateParagraph();
                XWPFRun r2 = p2.CreateRun();
                r2.FontFamily = "Times New Roman";
                r2.FontSize = 14;
                r2.IsBold = false;
                r2.SetText("Для совершения платежа, заключение договора не требуется.");

                p2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun r3 = p2.CreateRun();
                r3.FontFamily = "Times New Roman";
                r3.FontSize = 14;
                r3.IsBold = false;
                r3.SetText("На дату совершения оплаты срок действия договора не истёк.");

                //Вставляем параграф ----------------------------------------------
                var qp7 = doc.CreateParagraph();
                XWPFRun qr7 = qp7.CreateRun();
                qr7.FontFamily = "Times New Roman";
                qr7.FontSize = 14;
                qr7.IsBold = false;
                qr7.SetText("Реквизиты для платежа:");
                qr7.SetUnderline(UnderlinePatterns.Single);

                qp7.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr7R = qp7.CreateRun();
                qr7R.FontFamily = "Times New Roman";
                qr7R.FontSize = 14;
                qr7R.IsBold = false;
                qr7R.SetText("Получатель: ");
                qr7R.SetUnderline(UnderlinePatterns.Single);

                XWPFRun qr7RR = qp7.CreateRun();
                qr7RR.FontFamily = "Times New Roman";
                qr7RR.FontSize = 14;
                qr7RR.IsBold = true;
                if(zay.TurOpe != null)
                {
                    qr7RR.SetText(zay.TurOpe.Name);
                }
                else
                {
                    qr7RR.SetText(zay.Sanatorium.Name);
                }                
                //--------------------------------------------------------------
                string YUAddres = "";
                string POSTAddress = "";
                string RS = "";
                string BANK = "";
                string BANKAddress = ""
;               string BIC = "";
                string UNP = "";
                string OKPO = "";
                //-------------------------------------------
                if(zay.TurOpeId != null)
                {
                    if (zay.TurOpe.Address == null)
                    {
                        YUAddres = zay.TurOpe.City.Country.Name.ToString() + ", " + zay.TurOpe.City.Name.ToString();
                    }
                    else
                    {
                        YUAddres = zay.TurOpe.City.Country.Name.ToString() + ", " + zay.TurOpe.City.Name.ToString() + ", " + zay.TurOpe.Address.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.PostAddress == null)
                    {
                        POSTAddress = zay.TurOpe.City.Country.Name.ToString() + ", " + zay.TurOpe.City.Name.ToString();
                    }
                    else
                    {
                        POSTAddress = zay.TurOpe.City.Country.Name.ToString() + ", " + zay.TurOpe.City.Name.ToString() + ", " + zay.TurOpe.PostAddress.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.SanatInd == null)
                    {
                        RS = "";
                    }
                    else
                    {
                        RS = zay.TurOpe.SanatInd.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.Bank.Name == null)
                    {
                        BANK = "";
                    }
                    else
                    {
                        BANK = zay.TurOpe.Bank.Name.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.Bank.Address == null)
                    {
                        BANKAddress = "";
                    }
                    else
                    {
                        BANKAddress = zay.TurOpe.Bank.Address.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.Bank.Bic == null)
                    {
                        BIC = "";
                    }
                    else
                    {
                        BIC = "BIC: " + zay.TurOpe.Bank.Bic.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.Unp == null)
                    {
                        UNP = "";
                    }
                    else
                    {
                        UNP = "УНП: " + zay.TurOpe.Unp.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.Bank.Okpo == null)
                    {
                        OKPO = "";
                    }
                    else
                    {
                        OKPO = "ОКПО: " + zay.TurOpe.Bank.Okpo.ToString();
                    }
                }
                else
                {
                    if (zay.Sanatorium.Address == null)
                    {
                        YUAddres = zay.Sanatorium.City.Country.Name.ToString() + ", " + zay.Sanatorium.City.Name.ToString();
                    }
                    else
                    {
                        YUAddres = zay.Sanatorium.City.Country.Name.ToString() + ", " + zay.Sanatorium.City.Name.ToString() + ", " + zay.Sanatorium.Address.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.PostAddress == null)
                    {
                        POSTAddress = zay.Sanatorium.City.Country.Name.ToString() + ", " + zay.Sanatorium.City.Name.ToString();
                    }
                    else
                    {
                        POSTAddress = zay.Sanatorium.City.Country.Name.ToString() + ", " + zay.Sanatorium.City.Name.ToString() + ", " + zay.Sanatorium.PostAddress.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.SanatInd == null)
                    {
                        RS = "";
                    }
                    else
                    {
                        RS = zay.Sanatorium.SanatInd.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.Bank.Name == null)
                    {
                        BANK = "";
                    }
                    else
                    {
                        BANK = zay.Sanatorium.Bank.Name.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.Bank.Address == null)
                    {
                        BANKAddress = "";
                    }
                    else
                    {
                        BANKAddress = zay.Sanatorium.Bank.Address.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.Bank.Bic == null)
                    {
                        BIC = "";
                    }
                    else
                    {
                        BIC = "BIC: " + zay.Sanatorium.Bank.Bic.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.Unp == null)
                    {
                        UNP = "";
                    }
                    else
                    {
                        UNP = "УНП: " + zay.Sanatorium.Unp.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.Bank.Okpo == null)
                    {
                        OKPO = "";
                    }
                    else
                    {
                        OKPO = "ОКПО: " + zay.Sanatorium.Bank.Okpo.ToString();
                    }
                }                
                //-------------------------------------------

                qp7.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr7RRR = qp7.CreateRun();
                qr7RRR.FontFamily = "Times New Roman";
                qr7RRR.FontSize = 14;
                qr7RRR.IsBold = false;
                qr7RRR.SetText("Юр.адрес: " + YUAddres + ".");

                qp7.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr7RRRR = qp7.CreateRun();
                qr7RRRR.FontFamily = "Times New Roman";
                qr7RRRR.FontSize = 14;
                qr7RRRR.IsBold = false;
                qr7RRRR.SetText("Почт.адрес: " + POSTAddress + ".");

                qp7.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr7RRRRR = qp7.CreateRun();
                qr7RRRRR.FontFamily = "Times New Roman";
                qr7RRRRR.FontSize = 14;
                qr7RRRRR.IsBold = false;
                qr7RRRRR.SetText("Р/С: " + RS.Trim() + " в " + BANK.Trim() + ", " + BANKAddress.Trim() + ", " + BIC.Trim() + ", " + UNP.Trim() + ", " + OKPO.Trim() + ".");


                //--------------------------------------------------------------

                //----Вставляем таблицу с подписью подписанта------------------------------------------------------------------
                XWPFTable table3 = doc.CreateTable(1, 3);
                table3.Width = 5000;

                table3.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para31 = table3.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para32 = table3.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFParagraph para33 = table3.GetRow(0).GetCell(2).Paragraphs[0];

                XWPFRun run31 = para31.CreateRun();
                run31.FontFamily = "Times New Roman";
                run31.FontSize = 14;
                run31.SetText(UsFil.Employee.Position.Name);

                para31.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run31R = para31.CreateRun();
                run31R.FontFamily = "Times New Roman";
                run31R.FontSize = 14;
                run31R.SetText(UsFil.Employee.Department.Name);

                XWPFRun run32 = para32.CreateRun();
                run32.FontFamily = "Times New Roman";
                run32.FontSize = 14;
                run32.SetText("");

                para32.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run32R = para32.CreateRun();
                run32R.FontFamily = "Times New Roman";
                run32R.FontSize = 14;
                run32R.SetText("\t\t____________________");

                XWPFRun run33 = para33.CreateRun();
                run33.FontFamily = "Times New Roman";
                run33.FontSize = 14;
                run33.SetText("");

                para33.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run33R = para33.CreateRun();
                run33R.FontFamily = "Times New Roman";
                run33R.FontSize = 14;
                run33R.SetText("\t\t" + UsFil.Employee.FirstName + " " + UsFil.Employee.LastName.Substring(0, 1) + "." + UsFil.Employee.MiddleName.Substring(0, 1) + ".");

                //-------------------------------------------------------------------------------------------

                doc.Write(fs);
                //fs.Close();
            }

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "C:/Install/ReportOplata.docx");
            FileStream fs1 = new FileStream(path, FileMode.OpenOrCreate);
            string file_type = "text/plain";
            string file_name = "Докладная записка на оплату №_" + zay.NumberZ + "_от_" + Convert.ToDateTime(zay.DateZ).ToString("d") + ".docx";

            return File(fs1, file_type, file_name);

        }
        //-------------------------------------------------------------------------------------------

        //------------------------------Печать докладной записки на оплату дополнительных услуг---------------------------------------------
        public ActionResult ReportDopOplata(int ID)
        {
            Zayavlenie zay = new Zayavlenie();
            zay = db.Zayavlenie.Include(t => t.Employee.Filial).Include(o => o.Employee.Position).Include(ee => ee.Employee.Department).Include(p => p.TableZay).Include(yu => yu.Protocol).Include(h => h.Sanatorium.City.Country).Include(yy => yy.Sanatorium.Bank).Include(t=>t.TurOpe.City.Country).Include(tt =>tt.TurOpe.Bank).FirstOrDefault(g => g.Id == ID);

            List<Komissiya> listkom = new List<Komissiya>();
            listkom = db.Komissiya.Include(h => h.Status).Include(w => w.Employee.Position).Include(e => e.Employee.Department).OrderBy(u => u.Status.Priznak).Where(t => t.Employee.FilialId == Convert.ToInt32(zay.Employee.FilialId)).ToList();

            Komissiya ruk = listkom.FirstOrDefault(j => j.Status.Name.Trim() == "Руководитель");

            TimeSpan? DaysRest = zay.Po - zay.S;

            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee.Department).Include(t => t.Employee.Position).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            //----Выводим список родственников(детей) согласно заявления---------------------------
            List<TableZay> listTabZay = new List<TableZay>();
            listTabZay = db.TableZay.Include(y => y.Child).Where(qq => qq.ZayId == ID).ToList();
            //-------------------------------------------------------------------------------------

            var newFile2 = @"C:/Install/ReportDopOplata.docx";
            //var newFile2 = @"wwwroot/Report/ReportDopOplata.docx";
            using (var fs = new FileStream(newFile2, FileMode.Create, FileAccess.Write))
            {
                XWPFDocument doc = new XWPFDocument();

                //----Заполняем шапку заявления-----------------------------------------------------------------------------------------------------------------------------------------------------

                XWPFTable table1 = doc.CreateTable(2, 2);
                table1.Width = 5000;

                table1.SetColumnWidth(0, 2500);
                table1.SetColumnWidth(1, 2500);

                table1.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para1 = table1.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para2 = table1.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFParagraph para3 = table1.GetRow(1).GetCell(0).Paragraphs[0];
                XWPFParagraph para4 = table1.GetRow(1).GetCell(1).Paragraphs[0];

                XWPFRun run1 = para1.CreateRun();
                run1.FontFamily = "Times New Roman";
                run1.FontSize = 14;
                run1.SetText(UsFil.Employee.Department.Name);


                XWPFRun run2 = para2.CreateRun();
                run2.FontFamily = "Times New Roman";
                run2.FontSize = 14;
                if (ruk != null)
                {
                    run2.SetText(ruk.Employee.Position.Name);
                }
                else
                {
                    run2.SetText("");
                }

                para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2R = para2.CreateRun();
                run2R.FontFamily = "Times New Roman";
                run2R.FontSize = 14;
                if (ruk != null)
                {
                    run2R.SetText(ruk.Employee.FirstName + " " + ruk.Employee.LastName.Substring(0, 1) + "." + ruk.Employee.MiddleName.Substring(0, 1) + ".");
                }
                else
                {
                    run2R.SetText("");
                }

                XWPFRun run3 = para3.CreateRun();
                run3.FontFamily = "Times New Roman";
                run3.FontSize = 14;
                run3.SetText("ДОКЛАДНАЯ ЗАПИСКА\t\t");

                para3.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run3R = para3.CreateRun();
                run3R.FontFamily = "Times New Roman";
                run3R.FontSize = 14;
                if (UsFil.Employee.Department.Priznak != null)
                {
                    run3R.SetText(DateTime.Now.ToString("d") + " № " + UsFil.Employee.Department.Priznak + "/");
                }
                else
                {
                    run3R.SetText(DateTime.Now.ToString("d") + " № " + "/");
                }

                para3.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run3RR = para3.CreateRun();
                run3RR.FontFamily = "Times New Roman";
                run3RR.FontSize = 14;
                if (zay.Employee.Filial.Priznak != null)
                {
                    run3RR.SetText(zay.Employee.Filial.Priznak);
                }
                else
                {
                    run3RR.SetText("");
                }

                XWPFRun run4 = para4.CreateRun();
                run4.FontFamily = "Times New Roman";
                run4.IsItalic = true;
                run4.FontSize = 16;
                run4.SetText("Бухгалтерии");

                para4.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run4R = para4.CreateRun();
                run4R.FontFamily = "Times New Roman";
                run4R.IsItalic = true;
                run4R.FontSize = 16;
                run4R.SetText("к оплате");

                para4.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run4RR = para4.CreateRun();
                run4RR.FontFamily = "Times New Roman";
                run4RR.FontSize = 14;
                run4RR.SetText("_______________________");

                para4.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run4RRR = para4.CreateRun();
                run4RRR.FontFamily = "Times New Roman";
                run4RRR.FontSize = 14;
                run4RRR.SetText("____.____.20__");

                table1.SetColumnWidth(0, 2500);
                table1.SetColumnWidth(1, 2500);

                var p110 = doc.CreateParagraph();
                p110.Alignment = ParagraphAlignment.LEFT;
                //p111.IndentationFirstLine = 500;
                XWPFRun r110 = p110.CreateRun();
                r110.FontFamily = "Times New Roman";
                r110.FontSize = 14;
                r110.IsBold = false;
                r110.SetText("Об оплате в белорусских рублях.");

                string dog = "";
                if(zay.TurOpe != null)
                {
                 dog = "от " + Convert.ToDateTime(zay.DateDog).ToString("d") + " № " + zay.NumberDog;
                }
                else
                {
                Dogovor DogD = new Dogovor();
                DogD = db.Dogovor.FirstOrDefault(k => k.SanatoriumId == zay.SanatoriumId && k.PriznakClose != "Закрыт" && (k.TypeDogId == 2 || k.TypeDogId == 3) && k.FillialId == UsFil.Employee.FilialId);
                if (DogD != null)
                {
                    dog = "от " + Convert.ToDateTime(DogD.DateDog).ToString("d") + " № " + DogD.Number;
                }
                else
                {
                    dog = "";
                }
                }
                

                string sum = "";
                if (zay.SummaDop == null)
                {
                    sum = "";
                }
                else
                {
                    sum = zay.SummaDop.ToString() + " белорусских рублей, без НДС ";
                }

                //--Проверяем есть ли РП для сотрудника-----
                string sotr = "";
                if (zay.Employee.Fiorp == null || zay.Employee.Fiorp == "")
                {
                    sotr = zay.Employee.FirstName + " " + zay.Employee.LastName.Substring(0, 1) + "." + zay.Employee.MiddleName.Substring(0, 1) + ".";
                }
                else
                {
                    sotr = zay.Employee.Fiorp;
                }
                //------------------------------------------

                //------Проверяем на кого выписана путевка---------
                string who = "";
                if (zay.Who == "работника")
                {
                    who = "работника " + zay.Employee.Filial.Name + " " + sotr;
                }
                else if (zay.Who == "пенсионера")
                {
                    who = "пенсионера " + zay.Employee.Filial.Name + " " + sotr;
                }
                else if (zay.Who == "детей")
                {
                    string child = "";
                    foreach (var item in zay.TableZay)
                    {
                        child = child + " " + item.Child.Fio + " " + Convert.ToDateTime(item.Child.DateBirth).ToString("d") + ", ";
                    }
                    child = child.Remove(child.Length - 1);
                    if (zay.TableZay.Count() > 1)
                    {
                        who = "детей " + child + " работника " + sotr + ".";
                    }
                    else
                    {
                        who = "ребёнка " + child + " работника " + sotr + ".";
                    }
                }
                else
                {
                    string child = "";
                    foreach (var item in zay.TableZay)
                    {
                        child = child + " " + item.Child.Fio + " " + Convert.ToDateTime(item.Child.DateBirth).ToString("d") + " г.р., ";
                    }
                    string childRem = child.Remove(child.Length - 2);
                    child = child.Remove(child.Length - 1);
                    if (zay.TableZay.Count() > 1)
                    {
                        who = " работника " + sotr + " и его детей " + child + ".";
                    }
                    else
                    {
                        who = " работника " + sotr + " и его ребёнка " + child + ".";
                    }
                }
                //-------------------------------------------------
                //Вставляем параграф с суммой----------------------------------------------
                var qp1 = doc.CreateParagraph();
                qp1.Alignment = ParagraphAlignment.BOTH;
                qp1.IndentationFirstLine = 500;
                XWPFRun qr1 = qp1.CreateRun();
                qr1.FontFamily = "Times New Roman";
                qr1.FontSize = 14;
                qr1.IsBold = false;
                if(zay.TurOpe != null)
                {
                qr1.SetText("В соответствии с договором на оказание дополнительных медицинских услуг " + dog + ", заключенным с " + zay.TurOpe.Name + ", прошу Вас дать поручение бухгалтерии оплатить сумму " + sum + " (" + RusCurrency.Str(Convert.ToDouble(zay.SummaDop)) + ") за оздоровление и санаторно-курортное лечение " + who);
                }
                else
                {
                qr1.SetText("В соответствии с договором на оказание дополнительных медицинских услуг " + dog + ", заключенным с " + zay.Sanatorium.Name + ", прошу Вас дать поручение бухгалтерии оплатить сумму " + sum + " (" + RusCurrency.Str(Convert.ToDouble(zay.SummaDop)) + ") за оздоровление и санаторно-курортное лечение " + who);
                }
                
                //--------------------------------------------------------------

                //Вставляем параграф с условиями оплаты----------------------------------------------
                var qp2 = doc.CreateParagraph();
                //qp2.Alignment = ParagraphAlignment.BOTH;
                //qp2.IndentationFirstLine = 500;
                XWPFRun qr2 = qp2.CreateRun();
                qr2.FontFamily = "Times New Roman";
                qr2.FontSize = 14;
                qr2.IsBold = false;
                qr2.SetText("Условия оплаты: ");
                qr2.SetUnderline(UnderlinePatterns.Single);
                //--------------------------------------------------------------

                XWPFRun qr2R = qp2.CreateRun();
                qr2R.FontFamily = "Times New Roman";
                qr2R.FontSize = 14;
                qr2R.IsBold = false;
                qr2R.SetText("Предоплата.");

                qp2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr3 = qp2.CreateRun();
                qr3.FontFamily = "Times New Roman";
                qr3.FontSize = 14;
                qr3.IsBold = false;
                qr3.SetText("Основание для оплаты: ");
                qr3.SetUnderline(UnderlinePatterns.Single);
                //--------------------------------------------------------------

                XWPFRun qr3R = qp2.CreateRun();
                qr3R.FontFamily = "Times New Roman";
                qr3R.FontSize = 14;
                qr3R.IsBold = false;
                qr3R.SetText("Договор на оказание санаторно-курортных услуг " + dog + ", счет-фактура, заявление " + sotr + " № " + zay.NumberZ + " от " + Convert.ToDateTime(zay.DateZ).ToString("d"));
                //-----------------------------

                qp2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr4 = qp2.CreateRun();
                qr4.FontFamily = "Times New Roman";
                qr4.FontSize = 14;
                qr4.IsBold = false;
                qr4.SetText("Срок оплаты:");
                qr4.SetUnderline(UnderlinePatterns.Single);
                //--------------------------------------------------------------

                qp2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr5 = qp2.CreateRun();
                qr5.FontFamily = "Times New Roman";
                qr5.FontSize = 14;
                qr5.IsBold = false;
                qr5.SetText("Обоснование цены: ");
                qr5.SetUnderline(UnderlinePatterns.Single);
                //--------------------------------------------------------------

                XWPFRun qr5R = qp2.CreateRun();
                qr5R.FontFamily = "Times New Roman";
                qr5R.FontSize = 14;
                qr5R.IsBold = false;
                qr5R.SetText("Договор на оказание санаторно-курортных услуг " + dog + ", счет-фактура");
                //-------------------

                qp2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr55 = qp2.CreateRun();
                qr55.FontFamily = "Times New Roman";
                qr55.FontSize = 14;
                qr55.IsBold = false;
                qr55.SetText("Код назначения платежа: ");
                qr55.SetUnderline(UnderlinePatterns.Single);
                //--------------------------------------------------------------

                XWPFRun qr55R5 = qp2.CreateRun();
                qr55R5.FontFamily = "Times New Roman";
                qr55R5.FontSize = 14;
                qr55R5.IsBold = false;
                qr55R5.SetText("20701 ");
                //-------------------

                //-----Заполняем общий текст в параграфе------------------------------------------------------------------------
                var p1 = doc.CreateParagraph();
                p1.Alignment = ParagraphAlignment.BOTH;
                p1.IndentationFirstLine = 500;
                XWPFRun r1 = p1.CreateRun();
                r1.FontFamily = "Times New Roman";
                r1.FontSize = 14;
                r1.IsBold = false;
                if(zay.TurOpe != null)
                {
r1.SetText("Сведения о нахождении " + zay.TurOpe.Name + " в перечне организаций и индивидуальных предпринимателей, в отношении которых ДФР КГК Республики Беларусь составлено заключение об установлении оснований, указанных в пункте 4 статьи 33 Налогового кодекса Республики Беларусь, на официальном сайте Комитета государственного контроля Республики Беларусь www.kgk.gov.by/ru/perechen_lzhestruktur-ru/ по состоянию на " + DateTime.Now.ToString("d") + " отсутствуют.");
                }
                else
                {
r1.SetText("Сведения о нахождении " + zay.Sanatorium.Name + " в перечне организаций и индивидуальных предпринимателей, в отношении которых ДФР КГК Республики Беларусь составлено заключение об установлении оснований, указанных в пункте 4 статьи 33 Налогового кодекса Республики Беларусь, на официальном сайте Комитета государственного контроля Республики Беларусь www.kgk.gov.by/ru/perechen_lzhestruktur-ru/ по состоянию на " + DateTime.Now.ToString("d") + " отсутствуют.");
                }
                
                var p2 = doc.CreateParagraph();
                XWPFRun r2 = p2.CreateRun();
                r2.FontFamily = "Times New Roman";
                r2.FontSize = 14;
                r2.IsBold = false;
                r2.SetText("Для совершения платежа, заключение договора не требуется.");

                p2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun r3 = p2.CreateRun();
                r3.FontFamily = "Times New Roman";
                r3.FontSize = 14;
                r3.IsBold = false;
                r3.SetText("На дату совершения оплаты срок действия договора не истёк.");

                //Вставляем параграф ----------------------------------------------
                var qp7 = doc.CreateParagraph();
                XWPFRun qr7 = qp7.CreateRun();
                qr7.FontFamily = "Times New Roman";
                qr7.FontSize = 14;
                qr7.IsBold = false;
                qr7.SetText("Реквизиты для платежа:");
                qr7.SetUnderline(UnderlinePatterns.Single);

                qp7.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr7R = qp7.CreateRun();
                qr7R.FontFamily = "Times New Roman";
                qr7R.FontSize = 14;
                qr7R.IsBold = false;
                qr7R.SetText("Получатель: ");
                qr7R.SetUnderline(UnderlinePatterns.Single);

                XWPFRun qr7RR = qp7.CreateRun();
                qr7RR.FontFamily = "Times New Roman";
                qr7RR.FontSize = 14;
                qr7RR.IsBold = true;
                if(zay.TurOpeId != null)
                {
                   qr7RR.SetText(zay.TurOpe.Name);
                }
                else
                {
                    qr7RR.SetText(zay.Sanatorium.Name);
                }
                

                //--------------------------------------------------------------
                string YUAddres = "";
                string POSTAddress = "";
                string RS = "";
                string BANK = "";
                string BANKAddress = "";
                string BIC = "";
                string UNP = "";
                string OKPO = "";
                //-------------------------------------------

                if (zay.TurOpeId != null)
                {
                    if (zay.TurOpe.Address == null)
                    {
                        YUAddres = zay.TurOpe.City.Country.Name.ToString() + ", " + zay.TurOpe.City.Name.ToString();
                    }
                    else
                    {
                        YUAddres = zay.TurOpe.City.Country.Name.ToString() + ", " + zay.TurOpe.City.Name.ToString() + ", " + zay.TurOpe.Address.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.PostAddress == null)
                    {
                        POSTAddress = zay.TurOpe.City.Country.Name.ToString() + ", " + zay.TurOpe.City.Name.ToString();
                    }
                    else
                    {
                        POSTAddress = zay.TurOpe.City.Country.Name.ToString() + ", " + zay.TurOpe.City.Name.ToString() + ", " + zay.TurOpe.PostAddress.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.SanatInd == null)
                    {
                        RS = "";
                    }
                    else
                    {
                        RS = zay.TurOpe.SanatInd.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.Bank.Name == null)
                    {
                        BANK = "";
                    }
                    else
                    {
                        BANK = zay.TurOpe.Bank.Name.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.Bank.Address == null)
                    {
                        BANKAddress = "";
                    }
                    else
                    {
                        BANKAddress = zay.TurOpe.Bank.Address.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.Bank.Bic == null)
                    {
                        BIC = "";
                    }
                    else
                    {
                        BIC = "BIC: " + zay.TurOpe.Bank.Bic.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.Unp == null)
                    {
                        UNP = "";
                    }
                    else
                    {
                        UNP = "УНП: " + zay.TurOpe.Unp.ToString();
                    }
                    //-------------------------------------------
                    if (zay.TurOpe.Bank.Okpo == null)
                    {
                        OKPO = "";
                    }
                    else
                    {
                        OKPO = "ОКПО: " + zay.TurOpe.Bank.Okpo.ToString();
                    }
                }
                else
                {
                    if (zay.Sanatorium.Address == null)
                    {
                        YUAddres = zay.Sanatorium.City.Country.Name.ToString() + ", " + zay.Sanatorium.City.Name.ToString();
                    }
                    else
                    {
                        YUAddres = zay.Sanatorium.City.Country.Name.ToString() + ", " + zay.Sanatorium.City.Name.ToString() + ", " + zay.Sanatorium.Address.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.PostAddress == null)
                    {
                        POSTAddress = zay.Sanatorium.City.Country.Name.ToString() + ", " + zay.Sanatorium.City.Name.ToString();
                    }
                    else
                    {
                        POSTAddress = zay.Sanatorium.City.Country.Name.ToString() + ", " + zay.Sanatorium.City.Name.ToString() + ", " + zay.Sanatorium.PostAddress.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.SanatInd == null)
                    {
                        RS = "";
                    }
                    else
                    {
                        RS = zay.Sanatorium.SanatInd.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.Bank.Name == null)
                    {
                        BANK = "";
                    }
                    else
                    {
                        BANK = zay.Sanatorium.Bank.Name.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.Bank.Address == null)
                    {
                        BANKAddress = "";
                    }
                    else
                    {
                        BANKAddress = zay.Sanatorium.Bank.Address.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.Bank.Bic == null)
                    {
                        BIC = "";
                    }
                    else
                    {
                        BIC = "BIC: " + zay.Sanatorium.Bank.Bic.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.Unp == null)
                    {
                        UNP = "";
                    }
                    else
                    {
                        UNP = "УНП: " + zay.Sanatorium.Unp.ToString();
                    }
                    //-------------------------------------------
                    if (zay.Sanatorium.Bank.Okpo == null)
                    {
                        OKPO = "";
                    }
                    else
                    {
                        OKPO = "ОКПО: " + zay.Sanatorium.Bank.Okpo.ToString();
                    }
                }
                //-------------------------------------------

                qp7.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr7RRR = qp7.CreateRun();
                qr7RRR.FontFamily = "Times New Roman";
                qr7RRR.FontSize = 14;
                qr7RRR.IsBold = false;
                qr7RRR.SetText("Юр.адрес: " + YUAddres + ".");

                qp7.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr7RRRR = qp7.CreateRun();
                qr7RRRR.FontFamily = "Times New Roman";
                qr7RRRR.FontSize = 14;
                qr7RRRR.IsBold = false;
                qr7RRRR.SetText("Почт.адрес: " + POSTAddress + ".");

                qp7.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun qr7RRRRR = qp7.CreateRun();
                qr7RRRRR.FontFamily = "Times New Roman";
                qr7RRRRR.FontSize = 14;
                qr7RRRRR.IsBold = false;
                qr7RRRRR.SetText("Р/С: " + RS.Trim() + " в " + BANK.Trim() + ", " + BANKAddress.Trim() + ", " + BIC.Trim() + ", " + UNP.Trim() + ", " + OKPO.Trim() + ".");


                //--------------------------------------------------------------

                //----Вставляем таблицу с подписью подписанта------------------------------------------------------------------
                XWPFTable table3 = doc.CreateTable(1, 3);
                table3.Width = 5000;

                table3.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table3.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para31 = table3.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para32 = table3.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFParagraph para33 = table3.GetRow(0).GetCell(2).Paragraphs[0];

                XWPFRun run31 = para31.CreateRun();
                run31.FontFamily = "Times New Roman";
                run31.FontSize = 14;
                run31.SetText(UsFil.Employee.Position.Name);

                para31.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run31R = para31.CreateRun();
                run31R.FontFamily = "Times New Roman";
                run31R.FontSize = 14;
                run31R.SetText(UsFil.Employee.Department.Name);

                XWPFRun run32 = para32.CreateRun();
                run32.FontFamily = "Times New Roman";
                run32.FontSize = 14;
                run32.SetText("");

                para32.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run32R = para32.CreateRun();
                run32R.FontFamily = "Times New Roman";
                run32R.FontSize = 14;
                run32R.SetText("\t\t____________________");

                XWPFRun run33 = para33.CreateRun();
                run33.FontFamily = "Times New Roman";
                run33.FontSize = 14;
                run33.SetText("");

                para33.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run33R = para33.CreateRun();
                run33R.FontFamily = "Times New Roman";
                run33R.FontSize = 14;
                run33R.SetText("\t\t" + UsFil.Employee.FirstName + " " + UsFil.Employee.LastName.Substring(0, 1) + "." + UsFil.Employee.MiddleName.Substring(0, 1) + ".");

                //-------------------------------------------------------------------------------------------

                doc.Write(fs);
                //fs.Close();
            }

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "C:/Install/ReportDopOplata.docx");
            FileStream fs1 = new FileStream(path, FileMode.OpenOrCreate);
            string file_type = "text/plain";
            string file_name = "Докладная записка на оплату допмедуслуг №_" + zay.NumberZ + "_от_" + Convert.ToDateTime(zay.DateZ).ToString("d") + ".docx";

            return File(fs1, file_type, file_name);

        }
        //-------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------

        //-----Формирование отчёта-----------//
        [HttpPost]
        public ActionResult ReportView([FromBody] ReportView mod)
        {
            List<Zayavlenie> LZ = new List<Zayavlenie>();

            foreach (var it in mod.Rol)
            {
                List<Zayavlenie> LZL = new List<Zayavlenie>();
                LZL = db.Zayavlenie.Include(r => r.TableZay).Include(t => t.Employee.Filial).Include(r => r.Protocol).Include(y => y.Sanatorium).Where(d => d.S >= mod.DateS && d.S <= mod.DatePO && d.Protocol.Filial.Id == it).ToList();
                LZ.AddRange(LZL);
            }

            //LZ = db.Zayavlenie.Include(r => r.TableZay).Include(t =>t.Employee.Filial).Include(r=>r.Protocol).Include(y=>y.Sanatorium).Where(d => d.DateZ >= mod.DateS && d.DateZ <= mod.DatePO).ToList();
            var fil = LZ.GroupBy(p => p.Protocol.Filial.Name);

            ViewBag.fil = fil;
            return PartialView(fil);
        }
        [HttpPost]
        //------------------------------Печать отчёта---------------------------------------------
        public ActionResult ReportDocx([FromBody] ReportView mod)

        {
            List<Zayavlenie> LZ = new List<Zayavlenie>();

            foreach (var it in mod.Rol)
            {
                List<Zayavlenie> LZL = new List<Zayavlenie>();
                LZL = db.Zayavlenie.Include(r => r.TableZay).Include(t => t.Employee.Filial).Include(r => r.Protocol).Include(y => y.Sanatorium.City).Where(d => d.S >= mod.DateS && d.S <= mod.DatePO && d.Protocol.Filial.Id == it).ToList();
                LZ.AddRange(LZL);
            }
            var fil = LZ.GroupBy(p => p.Protocol.Filial.Name);

            //var newFile2 = @"/wwwroot/Report/ReportHealth.docx";
            var newFile2 = @"wwwroot/Report/ReportHealth.docx"; 
            //var newFile2 = @"C:/Install/ReportHealth.docx";
            using (var fs = new FileStream(newFile2, FileMode.Create, FileAccess.Write))
            {
                XWPFDocument doc = new XWPFDocument();

                //----Заполняем шапку заявления-----------------------------------------------------------------------------------------------------------------------------------------------------

                XWPFTable table1 = doc.CreateTable(2, 2);
                table1.Width = 5000;

                table1.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table1.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para1 = table1.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para2 = table1.GetRow(0).GetCell(1).Paragraphs[0];


                XWPFRun run1 = para1.CreateRun();
                run1.FontFamily = "Times New Roman";
                run1.FontSize = 12;
                run1.SetText("\t\t\t\t\t\t\t\t\t\t");

                XWPFRun run2 = para2.CreateRun();
                run2.FontFamily = "Times New Roman";
                run2.FontSize = 12;
                run2.SetText("Генеральному директору");

                para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2RRR = para2.CreateRun();
                run2RRR.FontFamily = "Times New Roman";
                run2RRR.FontSize = 12;
                run2RRR.SetText("ОАО \"Гомельтранснефть Дружба\"");

                para2.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2RRRR = para2.CreateRun();
                run2RRRR.FontFamily = "Times New Roman";
                run2RRRR.FontSize = 12;
                run2RRRR.SetText("Борисенко О.Л.");

                table1.SetColumnWidth(0, 2000);
                table1.SetColumnWidth(1, 3000);

                //----------Заполняем заголовок--------------------------------------------                
                var qp1 = doc.CreateParagraph();
                qp1.Alignment = ParagraphAlignment.BOTH;
                qp1.IndentationFirstLine = 500;
                XWPFRun qr1 = qp1.CreateRun();
                qr1.FontFamily = "Times New Roman";
                qr1.FontSize = 14;
                qr1.IsBold = false;
                qr1.SetText("Список работников (пенсионеров) и их детей, оздоровившихся с " + Convert.ToDateTime(mod.DateS).ToString("d") + " по " + Convert.ToDateTime(mod.DatePO).ToString("d"));
                                
                //----Заполняем таблицу с оздоровившимися сотрудниками--------------------------------------------------------------

                int ind2 = -1;
                int num = 0;

                int Rab = 0;
                int Pens = 0;
                int Deti = 0;

                int RabI = 0;
                int PensI = 0;
                int DetiI = 0;
                decimal? sumTotal = 0;
                decimal? sumItog = 0;

                XWPFTable table221;
                if (mod.Rol.Count > 1)
                {
                 table221 = doc.CreateTable(LZ.Count + mod.Rol.Count * 3 + 1, 5);
                }
                else
                {
                table221 = doc.CreateTable(LZ.Count + mod.Rol.Count * 3, 5);
                }
                
                table221.Width = 5000;

                foreach (var it in fil)
                {
                    ind2++;
                   
                    XWPFTableRow row0 = table221.GetRow(ind2);
                    row0.MergeCells(0, 4);
                    XWPFParagraph para2211000 = table221.GetRow(ind2).GetCell(0).Paragraphs[0];
                    para2211000.Alignment = ParagraphAlignment.CENTER;

                    XWPFRun run22111000 = para2211000.CreateRun();
                    run22111000.FontFamily = "Times New Roman";
                    run22111000.FontSize = 12;
                    run22111000.IsBold = true;
                    run22111000.SetText(it.Key);

                    ind2++;

                    XWPFParagraph para221100 = table221.GetRow(ind2).GetCell(0).Paragraphs[0];
                    XWPFParagraph para221200 = table221.GetRow(ind2).GetCell(1).Paragraphs[0];
                    XWPFParagraph para221300 = table221.GetRow(ind2).GetCell(2).Paragraphs[0];
                    XWPFParagraph para221400 = table221.GetRow(ind2).GetCell(3).Paragraphs[0];
                    XWPFParagraph para221500 = table221.GetRow(ind2).GetCell(4).Paragraphs[0];

                    XWPFRun run2211100 = para221100.CreateRun();
                    run2211100.FontFamily = "Times New Roman";
                    run2211100.FontSize = 12;
                    run2211100.SetText("№ п/п");

                    XWPFRun run221200 = para221200.CreateRun();
                    run221200.FontFamily = "Times New Roman";
                    run221200.FontSize = 12;
                    run221200.SetText("ФИО работника/ пенсионера");

                    XWPFRun run221300 = para221300.CreateRun();
                    run221300.FontFamily = "Times New Roman";
                    run221300.FontSize = 12;
                    run221300.SetText("Место оздоровления");

                    XWPFRun run221400 = para221400.CreateRun();
                    run221400.FontFamily = "Times New Roman";
                    run221400.FontSize = 12;
                    run221400.SetText("Дата С");

                    XWPFRun run221500 = para221500.CreateRun();
                    run221500.FontFamily = "Times New Roman";
                    run221500.FontSize = 12;
                    run221500.SetText("Дата По");

                    foreach (var item in it)
                    {
                        ind2++;
                        num++;
                        sumTotal = sumTotal + item.Summa + item.SummaDop;
                        if (item.Who == "работника")
                        {
                            Rab++;
                        }
                        if (item.Who == "пенсионера")
                        {
                            Pens++;
                        }
                        if (item.Who == "детей")
                        {
                            Deti++;
                        }
                        if (item.Who == "семейная")
                        {
                            Rab++;
                            Deti = Deti + item.TableZay.Count();
                        }
                        XWPFParagraph para2211 = table221.GetRow(ind2).GetCell(0).Paragraphs[0];
                        XWPFParagraph para2212 = table221.GetRow(ind2).GetCell(1).Paragraphs[0];
                        XWPFParagraph para2213 = table221.GetRow(ind2).GetCell(2).Paragraphs[0];
                        XWPFParagraph para2214 = table221.GetRow(ind2).GetCell(3).Paragraphs[0];
                        XWPFParagraph para2215 = table221.GetRow(ind2).GetCell(4).Paragraphs[0];

                        XWPFRun run2211 = para2211.CreateRun();
                        run2211.FontFamily = "Times New Roman";
                        run2211.FontSize = 12;
                        run2211.SetText(num.ToString());

                        XWPFRun run2212 = para2212.CreateRun();
                        run2212.FontFamily = "Times New Roman";
                        run2212.FontSize = 12;
                        run2212.SetText(item.Employee.FirstName + " " + item.Employee.LastName.Substring(0, 1) + "." + item.Employee.MiddleName.Substring(0, 1) + ".");

                        XWPFRun run2213 = para2213.CreateRun();
                        run2213.FontFamily = "Times New Roman";
                        run2213.FontSize = 12;
                        run2213.SetText(item.Sanatorium.Name + ", " + item.Sanatorium.City.Name);

                        XWPFRun run2214 = para2214.CreateRun();
                        run2214.FontFamily = "Times New Roman";
                        run2214.FontSize = 12;
                        run2214.SetText(Convert.ToDateTime(item.S).ToString("d"));

                        XWPFRun run2215 = para2215.CreateRun();
                        run2215.FontFamily = "Times New Roman";
                        run2215.FontSize = 12;
                        run2215.SetText(Convert.ToDateTime(item.Po).ToString("d"));
                    }
                    num = 0;
                    ind2++;

                    DetiI = DetiI + Deti;
                    RabI = RabI + Rab;
                    PensI = PensI + Pens;
                    sumItog = sumItog + sumTotal;

                    XWPFTableRow row1 = table221.GetRow(ind2);
                    row1.MergeCells(0, 4);
                    XWPFParagraph para22110000 = table221.GetRow(ind2).GetCell(0).Paragraphs[0];

                    XWPFRun run221110000 = para22110000.CreateRun();
                    run221110000.FontFamily = "Times New Roman";
                    run221110000.FontSize = 12;
                    run221110000.IsBold = true;
                    run221110000.SetText("ИТОГО: " + (Deti + Rab + Pens));

                    para22110000.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run2RRRR00 = para22110000.CreateRun();
                    run2RRRR00.FontFamily = "Times New Roman";
                    run2RRRR00.FontSize = 12;
                    run2RRRR00.IsBold = true;
                    run2RRRR00.SetText("Работников: " + Rab);

                    para22110000.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run2RRRR000 = para22110000.CreateRun();
                    run2RRRR000.FontFamily = "Times New Roman";
                    run2RRRR000.FontSize = 12;
                    run2RRRR000.IsBold = true;
                    run2RRRR000.SetText("Детей: " + Deti);

                    para22110000.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run2RRRR0000 = para22110000.CreateRun();
                    run2RRRR0000.FontFamily = "Times New Roman";
                    run2RRRR0000.FontSize = 12;
                    run2RRRR0000.IsBold = true;
                    run2RRRR0000.SetText("Пенсионеров: " + Pens);

                    para22110000.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun run2RRRR0000w = para22110000.CreateRun();
                    run2RRRR0000w.FontFamily = "Times New Roman";
                    run2RRRR0000w.FontSize = 12;
                    run2RRRR0000w.IsBold = true;
                    run2RRRR0000w.SetText("На сумму: " + sumTotal);

                    Deti = 0;
                    Rab = 0;
                    Pens = 0;
                    sumTotal = 0;
                }

                if(mod.Rol.Count() >1)
                {
                  //----Вывод общих итогов если выбран не один филиал------------------
                ind2++;
                XWPFTableRow row2 = table221.GetRow(ind2);
                row2.MergeCells(0, 4);
                XWPFParagraph paraI = table221.GetRow(ind2).GetCell(0).Paragraphs[0];

                XWPFRun runI = paraI.CreateRun();
                runI.FontFamily = "Times New Roman";
                runI.FontSize = 12;
                runI.IsBold = true;
                runI.SetText("ИТОГО: " + (DetiI + RabI + PensI));

                paraI.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun runII = paraI.CreateRun();
                runII.FontFamily = "Times New Roman";
                runII.FontSize = 12;
                runII.IsBold = true;
                runII.SetText("Работников: " + RabI);

                paraI.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun runIII = paraI.CreateRun();
                runIII.FontFamily = "Times New Roman";
                runIII.FontSize = 12;
                runIII.IsBold = true;
                runIII.SetText("Детей: " + DetiI);

                paraI.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun runIIII = paraI.CreateRun();
                runIIII.FontFamily = "Times New Roman";
                runIIII.FontSize = 12;
                runIIII.IsBold = true;
                runIIII.SetText("Пенсионеров: " + PensI);

                    paraI.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                    XWPFRun runIIIIw = paraI.CreateRun();
                    runIIIIw.FontFamily = "Times New Roman";
                    runIIIIw.FontSize = 12;
                    runIIIIw.IsBold = true;
                    runIIIIw.SetText("На сумму: " + sumItog);
                }
                
                //----------------------------------------------------------------- 

                table221.SetColumnWidth(0, 100);
                table221.SetColumnWidth(1, 1400);
                table221.SetColumnWidth(2, 1500);
                table221.SetColumnWidth(3, 1000);
                table221.SetColumnWidth(4, 1000);
                //---------------------------------------------------------------------------

                //-------Выводим подпись секретаря комиссии---------------------------------

                var p1111 = doc.CreateParagraph();
                p1111.Alignment = ParagraphAlignment.LEFT;
                //p111.IndentationFirstLine = 500;
                XWPFRun r1111 = p1111.CreateRun();
                r1111.FontFamily = "Times New Roman";
                r1111.FontSize = 12;
                r1111.IsBold = false;
                r1111.SetText("");


                //----Выводим подпись председателя комиссии--------------------------------------------------
                User UsFil = new User();
                UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

                List<Komissiya> listkom = new List<Komissiya>();
                //listkom = db.Komissiya.Where(g => g.Employee.Filial.Id == UsFil.Employee.Filial.Id).Include(y => y.Employee).ToList();
                //Необходимо проверить сколько чекбоксов выбрано и если один - выбрать комиссию согласно выбранному чекбокусу, если больше одного чекбокса - выбрать комиссию пользователя, печатающего отчет.
                if(mod.Rol.Count > 1 || mod.Rol.Count == 0)
                {
                listkom = db.Komissiya.Include(y => y.Employee).Where(g => g.Employee.FilialId == UsFil.Employee.FilialId).ToList();
                }
                else
                {
                listkom = db.Komissiya.Include(y => y.Employee).Where(g => g.Employee.FilialId == Convert.ToInt32(mod.Rol.FirstOrDefault())).ToList();
                    
                }
                
                Employee podpisant = new Employee();
                if (listkom.Where(t => t.Priznak == "1").Count() != 0)
                {
                    Komissiya kom = new Komissiya();
                    kom = listkom.FirstOrDefault(kk => kk.Priznak == "1");
                    podpisant = db.Employee.Include(j => j.Position).FirstOrDefault(y => y.Id == kom.EmployeeId);
                }
                else
                {
                    podpisant = db.Employee.Include(j => j.Position).FirstOrDefault(u => u.Id == UsFil.EmployeeId);
                }

                XWPFTable table5 = doc.CreateTable(1, 3);
                table5.Width = 5000;

                table5.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table5.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table5.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table5.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table5.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table5.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para51 = table5.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para52 = table5.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFParagraph para53 = table5.GetRow(0).GetCell(2).Paragraphs[0];

                XWPFRun run51 = para51.CreateRun();
                run51.FontFamily = "Times New Roman";
                run51.FontSize = 12;
                run51.SetText(podpisant.Position.Name);

                XWPFRun run52 = para52.CreateRun();
                run52.FontFamily = "Times New Roman";
                run52.FontSize = 12;
                run52.SetText("\t__________________");

                XWPFRun run53 = para53.CreateRun();
                run53.FontFamily = "Times New Roman";
                run53.FontSize = 12;
                run53.SetText("\t" + podpisant.FirstName + " " + podpisant.LastName.Substring(0, 1) + "." + podpisant.MiddleName.Substring(0, 1) + ".");

                //-------------------------------------------------------------------------------------------

                doc.Write(fs);
                doc.Close();
            }
            return Json(1);
                       
        }
        //-------------------------------------------------------------------------------------------
        //--------------------Договоры------------------------------------------------------------------
        //----------Добавление договора-----------------------//
        public ActionResult AddDogovor()
        {
            List<Sanatorium> listdogovor = new List<Sanatorium>();
            listdogovor = db.Sanatorium.OrderBy(h => h.Name).ToList();
            ViewBag.listdogovor = listdogovor;

            List<TypeDog> listType = new List<TypeDog>();
            listType = db.TypeDog.ToList();
            ViewBag.listType = listType;

            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления договора-----------//
        [HttpPost]
        public ActionResult DogovorSave([FromBody] Dogovor mod)
        {

            try
            {
                User u = new User();
                u = db.User.FirstOrDefault(g => g.Name == mod.UserModif);

                //Находим ID филиала пользователя
                Employee EMPL = new Employee();
                EMPL = db.Employee.FirstOrDefault(h => h.Id == u.EmployeeId);

                Dogovor us = new Dogovor();
                us.Number = mod.Number;
                us.DateDog = mod.DateDog;
                us.DateStart = mod.DateStart;
                us.DateEnd = mod.DateEnd;
                us.SanatoriumId = mod.SanatoriumId;
                us.UserModif = mod.UserModif;
                us.DateModif = DateTime.Now;
                us.TypeDogId = mod.TypeDogId;
                us.FillialId = EMPL.FilialId;
                us.PriznakKontrol = mod.PriznakKontrol;
                us.SummaDog = mod.SummaDog;
                db.Dogovor.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Договор успешно добавлен!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление договора//

        public ActionResult DeleteDogovor(int ID)
        {
            Dogovor DogDel = new Dogovor();
            DogDel = db.Dogovor.FirstOrDefault(a => a.Id == ID);
            return PartialView(DogDel);
        }
        //-----------------------------//

        // Подтверждение удаления договора//
        public ActionResult DeleteDogovorOK(int ID)
        {
            try
            {
                Dogovor DogDS = new Dogovor();
                DogDS = db.Dogovor.FirstOrDefault(a => a.Id == ID);
                db.Dogovor.Remove(DogDS);
                db.SaveChanges();

                ViewBag.Message = "Договор удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование договора//

        public ActionResult DogovorEdit(int ID)
        {
            List<Sanatorium> listsanatorium = new List<Sanatorium>();
            listsanatorium = db.Sanatorium.OrderBy(h => h.Name).ToList();
            ViewBag.listsanatorium = listsanatorium;

            List<TypeDog> listType = new List<TypeDog>();
            listType = db.TypeDog.ToList();
            ViewBag.listType = listType;

            Dogovor DogEd = new Dogovor();
            DogEd = db.Dogovor.FirstOrDefault(a => a.Id == ID);

            return PartialView(DogEd);
        }
        //-------------------------------//

        //Сохранение редактирования договора------------//
        [HttpPost]
        public ActionResult DogovorEditSave([FromBody] Dogovor modus)
        {
            try
            {
                Dogovor dogE = new Dogovor();
                dogE = db.Dogovor.FirstOrDefault(s => s.Id == modus.Id);

                dogE.Number = modus.Number;
                dogE.DateDog = modus.DateDog;
                dogE.DateStart = modus.DateStart;
                dogE.DateEnd = modus.DateEnd;
                dogE.SanatoriumId = modus.SanatoriumId;
                dogE.TypeDogId = modus.TypeDogId;
                dogE.PriznakKontrol = modus.PriznakKontrol;
                dogE.SummaDog = modus.SummaDog;
                if (modus.PriznakClose == "1")
                {
                    dogE.PriznakClose = "Закрыт";
                }
                else
                {
                    dogE.PriznakClose = null;
                }
                dogE.UserModif = modus.UserModif;
                dogE.DateModif = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "договор изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//

        //--Вывод БВ и прочей корректирующей херни---------------------------//
        public IActionResult Koef()
        {
            List<Koef> ListKoef = new List<Koef>();
            ListKoef = db.Koef.OrderByDescending(da => da.DateModif).ToList();
            return View(ListKoef);
        }
        //----------Добавление коректирующей херни-----------------------//

        public ActionResult AddKoef()
        {
            return PartialView();
        }
        //--------------------------//
        //-----Сохранение добавления коректирующей херни-----------//
        [HttpPost]
        public ActionResult KoefSave([FromBody] Koef mod)
        {

            try
            {
                Koef us = new Koef();
                us.Name = mod.Name;
                us.DateStart = mod.DateStart;
                us.Value = mod.Value;
                us.Control = mod.Control;
                us.Preduptejdenie = mod.Preduptejdenie;
                us.Primech = mod.Primech;
                us.UserModif = mod.UserModif;
                us.DateModif = DateTime.Now;
                db.Koef.Add(us);
                db.SaveChanges();

                ViewBag.Message = "Коеффициент успешно добавлен!";
            }
            catch (Exception ex)
            {


                ViewBag.Message = ex.ToString();
            }

            return PartialView();
        }

        // удаление коректирующей херни//

        public ActionResult DeleteKoef(int ID)
        {
            Koef koefDel = new Koef();
            koefDel = db.Koef.FirstOrDefault(a => a.Id == ID);
            return PartialView(koefDel);
        }
        //-----------------------------//

        // Подтверждение удаления коректирующей херни//
        public ActionResult DeleteKoefOK(int ID)
        {
            try
            {
                Koef koefDS = new Koef();
                koefDS = db.Koef.FirstOrDefault(a => a.Id == ID);
                db.Koef.Remove(koefDS);
                db.SaveChanges();

                ViewBag.Message = "Коэффициент удален!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка удаления";
            }

            return PartialView();
        }
        // Редактирование коректирующей херни//

        public ActionResult KoefEdit(int ID)
        {
            Koef koefEd = new Koef();
            koefEd = db.Koef.FirstOrDefault(a => a.Id == ID);

            return PartialView(koefEd);
        }
        //-------------------------------//

        //Сохранение редактирования коректирующей херни------------//
        [HttpPost]
        public ActionResult KoefEditSave([FromBody] Koef modus)
        {
            try
            {
                Koef koefE = new Koef();
                koefE = db.Koef.FirstOrDefault(s => s.Id == modus.Id);
                koefE.Name = modus.Name;
                koefE.DateStart = modus.DateStart;
                koefE.Value = modus.Value;
                koefE.Control = modus.Control;
                koefE.Preduptejdenie = modus.Preduptejdenie;
                koefE.Primech = modus.Primech;
                koefE.UserModif = modus.UserModif;
                koefE.DateModif = DateTime.Now;
                db.SaveChanges();

                ViewBag.Message = "Коэффициент изменен";

            }
            catch (Exception e)
            {
                ViewBag.Message = "Ошибка. Текст ошибки: " + e.ToString();
            }

            return PartialView();
        }
        //-----------------------------//
        //--Использование сумм по договорам---------------------------//
        public IActionResult ReportDog()
        {
            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            //var zaydog = db.VDogbyzay.Where(t => t.FillialId == UsFil.Employee.FilialId ).GroupBy(h => h.Number).Select(grt => new { numberD = grt.Key, spis = grt.Select(p => p) });
            List<SummDogZay> ListSumm = new List<SummDogZay>();
            List<SummDogZay> ListSummADD = new List<SummDogZay>();
            ListSumm = db.SummDogZay.Include(y => y.Dogovor).Include(h => h.Zayavlenie).Where(t => t.Zayavlenie.Protocol.FilialId == UsFil.Employee.FilialId).ToList();
            foreach (var item in ListSumm)
            {
                //находим разность дат в годах
                TimeSpan difference = DateTime.Now - Convert.ToDateTime(item.Dogovor.DateStart);
                int raznost = Convert.ToInt32(Math.Truncate(difference.TotalDays / 365.25));
                DateTime DatStart = Convert.ToDateTime(item.Dogovor.DateStart).AddYears(raznost);
                DateTime DatEnd = Convert.ToDateTime(item.Dogovor.DateStart).AddYears(raznost + 1).AddDays(-1);

                //if( item.Zayavlenie.DateZ >= DatStart && item.Zayavlenie.DateZ <= DatEnd )
                //{
                //    SummDogZay Sd = new SummDogZay();
                //        Sd = item;
                //     ListSummADD.Add(Sd);
                //}                
            }
            //Делаю выборку из БД
            List<SummDogZay> ListSumDogZayav = new List<SummDogZay>();
            ListSumDogZayav = db.SummDogZay.Include(y => y.Dogovor).Include(ty => ty.Zayavlenie.Protocol).Include(r => r.Zayavlenie.Sanatorium).Where(t => t.Zayavlenie.Protocol.FilialId == UsFil.Employee.FilialId).ToList();

            ListSummADD = ListSumDogZayav.Where(t => (t.Zayavlenie.DateZ >= t.Dogovor.DateStart.Value.AddYears(Convert.ToInt32(Math.Truncate((DateTime.Now - t.Dogovor.DateStart.Value).TotalDays / 365.25))) && t.Zayavlenie.DateZ <= t.Dogovor.DateStart.Value.AddYears(Convert.ToInt32(Math.Truncate((DateTime.Now - t.Dogovor.DateStart.Value).TotalDays / 365.25)) + 1).AddDays(-1))).ToList();
            var ListGroup = ListSummADD.GroupBy(hj => hj.Dogovor).Select(grt => new { numberD = grt.Key.Number, dateD = grt.Key.DateDog, sanatorium = grt.Key.Sanatorium.Name, sumList = grt.Sum(gh => gh.Summa), dateEND = grt.Key.DateStart.Value.AddYears(Convert.ToInt32(Math.Truncate((DateTime.Now - grt.Key.DateStart.Value).TotalDays / 365.25)) + 1).AddDays(-1), spis = grt.Select(p => p) }).OrderBy(gf => gf.sanatorium);
            ViewBag.ListGroup = ListGroup;
            List<Koef> listkoef = new List<Koef>();
            listkoef = db.Koef.Where(j => j.Name == "БВ").OrderByDescending(jk => jk.DateStart).ToList();
            ViewBag.K = listkoef.FirstOrDefault();
            return View();
        }

        //Получим список договоров с суммами соглассно санаторию
        public IActionResult SummDogList([FromBody] Zayavlenie spSum)
        {
            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Koef> LK = new List<Koef>();
            LK = db.Koef.Where(u => u.Name.Trim() == "БВ").OrderByDescending(t => t.DateStart).ToList();
            ViewBag.BV = LK.FirstOrDefault();
            
            List<Dogovor> Ldog = new List<Dogovor>();
            //Получим список договоров сглассно санаторию
            List<SummDogZay> LSDZ = new List<SummDogZay>();
            LSDZ = db.SummDogZay.Include(u => u.Dogovor).Include(i => i.Zayavlenie).Where(y => y.Dogovor.FillialId == UsFil.Employee.FilialId && y.Dogovor.PriznakClose != "Закрыт" && y.Zayavlenie.SanatoriumId == spSum.SanatoriumId).ToList();
            
            //Сгруппируем  по договорам заявления
            if (LSDZ.Count != 0)
            {
                if(LSDZ.FirstOrDefault().Dogovor.PriznakKontrol == 1)
                {
                 List<SummDogZay> LSDZSP = new List<SummDogZay>();
                 LSDZSP = LSDZ.Where(y => ((y.Zayavlenie.DateZ >= y.Dogovor.DateStart.Value.AddYears(Convert.ToInt32(Math.Truncate((DateTime.Now - y.Dogovor.DateStart.Value).TotalDays / 365.25))) && y.Zayavlenie.DateZ <= y.Dogovor.DateStart.Value.AddYears(Convert.ToInt32(Math.Truncate((DateTime.Now - y.Dogovor.DateStart.Value).TotalDays / 365.25)) + 1).AddDays(-1)))).ToList();
                        
                var listDogSumm = LSDZSP.GroupBy(hj => hj.Dogovor).Select(grt => new { numberD = grt.Key.Number, dateD = grt.Key.DateDog, sumList = grt.Sum(gh => gh.Summa), priznakkontrol = grt.Key.PriznakKontrol, sumdog = grt.Key.SummaDog });
                ViewBag.listDogSumm = listDogSumm;
                return PartialView();
                }

                if(LSDZ.FirstOrDefault().Dogovor.PriznakKontrol == 2)
                {   
                    var listDogSumm = LSDZ.GroupBy(hj => hj.Dogovor).Select(grt => new { numberD = grt.Key.Number, dateD = grt.Key.DateDog, sumList = grt.Sum(gh => gh.Summa), priznakkontrol = grt.Key.PriznakKontrol, sumdog = grt.Key.SummaDog });
                    ViewBag.listDogSumm = listDogSumm;
                    return PartialView();
                }

                if (LSDZ.FirstOrDefault().Dogovor.PriznakKontrol == 3)
                {
                    var listDogSumm = LSDZ.GroupBy(hj => hj.Dogovor).Select(grt => new { numberD = grt.Key.Number, dateD = grt.Key.DateDog, sumList = grt.Sum(gh => gh.Summa), priznakkontrol = grt.Key.PriznakKontrol, sumdog = grt.Key.SummaDog });
                    ViewBag.listDogSumm = listDogSumm;
                    return PartialView();
                }
                else
                {   
                    var listDogSumm = LSDZ.GroupBy(hj => hj.Dogovor).Select(grt => new { numberD = grt.Key.Number, dateD = grt.Key.DateDog, sumList = grt.Sum(gh => gh.Summa), priznakkontrol = grt.Key.PriznakKontrol, sumdog = grt.Key.SummaDog });
                    ViewBag.listDogSumm = listDogSumm;
                    return PartialView();
                }

            }
            else
            {
                Ldog = db.Dogovor.Include(yu => yu.Sanatorium).Where(y => y.FillialId == UsFil.Employee.FilialId && y.PriznakClose != "Закрыт" && y.SanatoriumId == spSum.SanatoriumId).ToList();
                var listDogSumm = Ldog.Select(grt => new { numberD = grt.Number, dateD = grt.DateDog, sumList = 0, priznakkontrol = grt.PriznakKontrol, sumdog =0 });
                ViewBag.listDogSumm = listDogSumm;
                return PartialView();
            }   

        }
        //----------Получаем список заявлений в зависимости от дат---------------------------------------------------------------
        [HttpPost]
        public ActionResult GetZayTab([FromBody] Zayavlenie ZayFilter)
        {
            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Zayavlenie> zayTab = new List<Zayavlenie>();

            if (ZayFilter.Priznak == "1")
            {
                zayTab = db.Zayavlenie.Include(y => y.TableZay).Include(h => h.Protocol).Include(g => g.Employee).Include(p => p.Sanatorium).Where(i => i.Protocol.Filial.Id == UsFil.Employee.FilialId && i.DateZ >= ZayFilter.S && i.DateZ <= ZayFilter.Po).OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }
            if (ZayFilter.Priznak == "2")
            {
                zayTab = db.Zayavlenie.Include(y => y.TableZay).Include(h => h.Protocol).Include(g => g.Employee).Include(p => p.Sanatorium).Where(i => i.Protocol.Filial.Id == UsFil.Employee.FilialId && i.DateZ >= ZayFilter.S && i.DateZ <= ZayFilter.Po && i.Priznak == "принято").OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }
            if (ZayFilter.Priznak == "3")
            {
                zayTab = db.Zayavlenie.Include(y => y.TableZay).Include(h => h.Protocol).Include(g => g.Employee).Include(p => p.Sanatorium).Where(i => i.Protocol.Filial.Id == UsFil.Employee.FilialId && i.DateZ >= ZayFilter.S && i.DateZ <= ZayFilter.Po && i.Priznak != "принято").OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }

            //---Теперь обработаем второй фильтр по оплате-------
            List<Zayavlenie> zayTabOplata = new List<Zayavlenie>();

            if (ZayFilter.PriznakOplata == 1)
            {
                zayTabOplata = zayTab.OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }
            if (ZayFilter.PriznakOplata == 2)
            {
                zayTabOplata = zayTab.Where(i => i.PriznakOplata == 1).OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }
            if (ZayFilter.PriznakOplata == 3)
            {
                zayTabOplata = zayTab.Where(i => i.PriznakOplata == 0).OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }

            //----------------------------------------------------
            return PartialView(zayTabOplata);
        }
        //-----------------------------//

        //----------Получаем список протоколов в зависимости от дат---------------------------------------------------------------
        [HttpPost]
        public ActionResult GetProtTab([FromBody] Zayavlenie ZayFilter)
        {
            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Protocol> protTab = new List<Protocol>();
            protTab = db.Protocol.Where(i => i.Filial.Id == UsFil.Employee.FilialId && i.DateProt >= ZayFilter.S && i.DateProt <= ZayFilter.Po).OrderByDescending(da => da.DateProt).ToList();

            return PartialView(protTab);
        }
        //-----------------------------//
        public IActionResult TopSanatorium()
        {
            var phone = HttpContext.User.FindAll(ClaimTypes.Role);
            List<Fillial> ListF = new List<Fillial>();
            //ListF = db.Fillial.ToList();
            foreach (var f in phone)
            {
                foreach (var item in db.Fillial.ToList())
                {
                    if (item.Id == Convert.ToInt32(f.Value))
                    {
                        ListF.Add(item);
                    }
                }
            }

            ViewBag.ListF = ListF;

            return View();
        }

        //-----Формирование отчёта-----------//
        [HttpPost]
        public ActionResult ReportViewTop([FromBody] ReportView mod)
        {
            List<Zayavlenie> LZ = new List<Zayavlenie>();

            foreach (var it in mod.Rol)
            {
                List<Zayavlenie> LZL = new List<Zayavlenie>();
                LZL = db.Zayavlenie.Include(r => r.TableZay).Include(t => t.Employee.Filial).Include(r => r.Protocol).Include(y => y.Sanatorium).Where(d => d.S >= mod.DateS && d.S <= mod.DatePO && d.Protocol.Filial.Id == it).ToList();
                LZ.AddRange(LZL);
            }
            //Список без группировки по филиалам
            ViewBag.LZ = LZ.GroupBy(h =>h.Sanatorium).Select(rt => new { sanatorium = rt.Key.Name, count = rt.Count()}).OrderByDescending(j =>j.count);
            //Запишем в строку список филиалов
           ViewBag.MaxCount = LZ.GroupBy(h => h.Sanatorium).Select(rt => new { sanatorium = rt.Key.Name, count = rt.Count() }).OrderByDescending(j => j.count).FirstOrDefault().count;

            return PartialView(LZ);
        }
        [HttpPost]
        //------------------------------Печать списка заявлений---------------------------------------------
        public ActionResult ReportZayDocx([FromBody] Zayavlenie mod)

        {
            User UsFil = new User();
            UsFil = db.User.Include(g => g.Employee).FirstOrDefault(k => k.Name == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Zayavlenie> zayTab = new List<Zayavlenie>();

            if (mod.Priznak == "1")
            {
                zayTab = db.Zayavlenie.Include(y => y.TableZay).Include(h => h.Protocol).Include(g => g.Employee).Include(p => p.Sanatorium.City).Where(i => i.Protocol.Filial.Id == UsFil.Employee.FilialId && i.DateZ >= mod.S && i.DateZ <= mod.Po).OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }
            if (mod.Priznak == "2")
            {
                zayTab = db.Zayavlenie.Include(y => y.TableZay).Include(h => h.Protocol).Include(g => g.Employee).Include(p => p.Sanatorium.City).Where(i => i.Protocol.Filial.Id == UsFil.Employee.FilialId && i.DateZ >= mod.S && i.DateZ <= mod.Po && i.Priznak == "принято").OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }
            if (mod.Priznak == "3")
            {
                zayTab = db.Zayavlenie.Include(y => y.TableZay).Include(h => h.Protocol).Include(g => g.Employee).Include(p => p.Sanatorium.City).Where(i => i.Protocol.Filial.Id == UsFil.Employee.FilialId && i.DateZ >= mod.S && i.DateZ <= mod.Po && i.Priznak != "принято").OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }

            //---Теперь обработаем второй фильтр по оплате-------
            List<Zayavlenie> zayTabOplata = new List<Zayavlenie>();

            if (mod.PriznakOplata == 1)
            {
                zayTabOplata = zayTab.OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }
            if (mod.PriznakOplata == 2)
            {
                zayTabOplata = zayTab.Where(i => i.PriznakOplata == 1).OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }
            if (mod.PriznakOplata == 3)
            {
                zayTabOplata = zayTab.Where(i => i.PriznakOplata == 0).OrderByDescending(da => da.DateZ).OrderByDescending(df => df.NumberZ).ToList();
            }

            //var fil = zayTabOplata.GroupBy(p => p.Protocol.Filial.Name);

            var newFile2 = @"wwwroot/Report/ReportZayavlenie.docx";            
            using (var fs = new FileStream(newFile2, FileMode.Create, FileAccess.Write))
            {
                XWPFDocument doc = new XWPFDocument();

                //----------Заполняем заголовок--------------------------------------------
                string Title = "";

                 if (mod.Priznak == "2")
                {
                    Title = "Список работников (пенсионеров), отчитавшихся за путевку";
                }
                else if (mod.Priznak == "3")
                {
                    Title = "Список работников (пенсионеров), не отчитавшихся за путевку";
                }
                else 
                {
                    Title = "Список работников (пенсионеров)";
                }

                var qp1 = doc.CreateParagraph();
                qp1.Alignment = ParagraphAlignment.CENTER;
                qp1.IndentationFirstLine = 500;
                XWPFRun qr1 = qp1.CreateRun();
                qr1.FontFamily = "Times New Roman";
                qr1.FontSize = 14;
                qr1.IsBold = false;
                qr1.SetText(Title);
                
                qp1.CreateRun().AddBreak(BreakType.TEXTWRAPPING);
                XWPFRun run2RRRR00 = qp1.CreateRun();
                run2RRRR00.FontFamily = "Times New Roman";
                run2RRRR00.FontSize = 12;
                run2RRRR00.IsBold = true;
                run2RRRR00.SetText("за период: с " + Convert.ToDateTime(mod.S).ToString("d") + " по " + Convert.ToDateTime(mod.Po).ToString("d"));
                //----Заполняем таблицу с оздоровившимися сотрудниками--------------------------------------------------------------

                int num = 0;
                
                XWPFTable table221;
                table221 = doc.CreateTable(zayTabOplata.Count + 1, 6);  
                table221.Width = 5000;
                                                    
                    XWPFParagraph para221100 = table221.GetRow(0).GetCell(0).Paragraphs[0];
                    XWPFParagraph para221200 = table221.GetRow(0).GetCell(1).Paragraphs[0];
                    XWPFParagraph para221300 = table221.GetRow(0).GetCell(2).Paragraphs[0];
                    XWPFParagraph para221400 = table221.GetRow(0).GetCell(3).Paragraphs[0];
                    XWPFParagraph para221500 = table221.GetRow(0).GetCell(4).Paragraphs[0];
                    XWPFParagraph para221600 = table221.GetRow(0).GetCell(5).Paragraphs[0];

                    XWPFRun run2211100 = para221100.CreateRun();
                    run2211100.FontFamily = "Times New Roman";
                    run2211100.FontSize = 12;
                    run2211100.SetText("№ п/п");

                    XWPFRun run221200 = para221200.CreateRun();
                    run221200.FontFamily = "Times New Roman";
                    run221200.FontSize = 12;
                    run221200.SetText("ФИО работника/ пенсионера");

                    XWPFRun run221300 = para221300.CreateRun();
                    run221300.FontFamily = "Times New Roman";
                    run221300.FontSize = 12;
                    run221300.SetText("Заявление");

                    XWPFRun run221400 = para221400.CreateRun();
                    run221400.FontFamily = "Times New Roman";
                    run221400.FontSize = 12;
                    run221400.SetText("Дата С");

                    XWPFRun run221500 = para221500.CreateRun();
                    run221500.FontFamily = "Times New Roman";
                    run221500.FontSize = 12;
                    run221500.SetText("Дни");

                    XWPFRun run221600 = para221600.CreateRun();
                    run221600.FontFamily = "Times New Roman";
                    run221600.FontSize = 12;
                    run221600.SetText("Место отдыха");

                foreach (var item in zayTabOplata)
                {
                    num++;
                        
                        XWPFParagraph para2211 = table221.GetRow(num).GetCell(0).Paragraphs[0];
                        XWPFParagraph para2212 = table221.GetRow(num).GetCell(1).Paragraphs[0];
                        XWPFParagraph para2213 = table221.GetRow(num).GetCell(2).Paragraphs[0];
                        XWPFParagraph para2214 = table221.GetRow(num).GetCell(3).Paragraphs[0];
                        XWPFParagraph para2215 = table221.GetRow(num).GetCell(4).Paragraphs[0];
                        XWPFParagraph para2216 = table221.GetRow(num).GetCell(5).Paragraphs[0];

                        XWPFRun run2211 = para2211.CreateRun();
                        run2211.FontFamily = "Times New Roman";
                        run2211.FontSize = 12;
                        run2211.SetText(num.ToString());

                        XWPFRun run2212 = para2212.CreateRun();
                        run2212.FontFamily = "Times New Roman";
                        run2212.FontSize = 12;
                        run2212.SetText(item.Employee.FirstName + " " + item.Employee.LastName.Substring(0, 1) + "." + item.Employee.MiddleName.Substring(0, 1) + ".");

                        XWPFRun run2213 = para2213.CreateRun();
                        run2213.FontFamily = "Times New Roman";
                        run2213.FontSize = 12;
                        run2213.SetText("№ " + item.NumberZ + " от " + Convert.ToDateTime(item.DateZ).ToString("d"));

                        XWPFRun run2214 = para2214.CreateRun();
                        run2214.FontFamily = "Times New Roman";
                        run2214.FontSize = 12;
                        run2214.SetText(Convert.ToDateTime(item.S).ToString("d"));

                        XWPFRun run2215 = para2215.CreateRun();
                        run2215.FontFamily = "Times New Roman";
                        run2215.FontSize = 12;
                        run2215.SetText(Convert.ToDateTime(item.Po).ToString("d"));

                        XWPFRun run2216 = para2216.CreateRun();
                        run2216.FontFamily = "Times New Roman";
                        run2216.FontSize = 12;
                    if(item.Anulirovano == 1)
                    {
                        run2216.SetText("Анулировано Протоколом комиссии!");
                    }
                    else
                    {
                    run2216.SetText(item.Sanatorium.Name + ", " + item.Sanatorium.City.Name);
                    }                       
                }
                
                //----------------------------------------------------------------- 

                table221.SetColumnWidth(0, 100);
                table221.SetColumnWidth(1, 1300);
                table221.SetColumnWidth(2, 800);
                table221.SetColumnWidth(3, 800);
                table221.SetColumnWidth(4, 800);
                table221.SetColumnWidth(5, 1200);
                //---------------------------------------------------------------------------
                //Вставляем пустой параграф
                var p110 = doc.CreateParagraph();
                p110.Alignment = ParagraphAlignment.LEFT;
                //p111.IndentationFirstLine = 500;
                XWPFRun r110 = p110.CreateRun();
                r110.FontFamily = "Times New Roman";
                r110.FontSize = 10;
                r110.IsBold = false;
                r110.SetText("");

                //List<Komissiya> podpisant = new List<Komissiya>();
                //podpisant = db.Komissiya.Where(k => k.Employee.Filial.Id == UsFil.Employee.Filial.Id).ToList();
                User podpisant =new User();
                List<User> users = new List<User>();
                users = db.User.Include(t =>t.Employee.Position).ToList();
                podpisant = users.FirstOrDefault(l => l.Name == UsFil.Name);

                XWPFTable table5 = doc.CreateTable(1, 3);
                table5.Width = 5000;

                table5.SetLeftBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table5.SetRightBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table5.SetBottomBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table5.SetTopBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table5.SetInsideHBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");
                table5.SetInsideVBorder(XWPFTable.XWPFBorderType.NONE, 4, 0, "FF0000");

                XWPFParagraph para51 = table5.GetRow(0).GetCell(0).Paragraphs[0];
                XWPFParagraph para52 = table5.GetRow(0).GetCell(1).Paragraphs[0];
                XWPFParagraph para53 = table5.GetRow(0).GetCell(2).Paragraphs[0];

                XWPFRun run51 = para51.CreateRun();
                run51.FontFamily = "Times New Roman";
                run51.FontSize = 12;
                run51.SetText(podpisant.Employee.Position.Name);

                XWPFRun run52 = para52.CreateRun();
                run52.FontFamily = "Times New Roman";
                run52.FontSize = 12;
                run52.SetText("\t__________________");

                XWPFRun run53 = para53.CreateRun();
                run53.FontFamily = "Times New Roman";
                run53.FontSize = 12;
                run53.SetText("\t" + podpisant.Employee.FirstName + " " + podpisant.Employee.LastName.Substring(0, 1) + "." + podpisant.Employee.MiddleName.Substring(0, 1) + ".");

                //-------------------------------------------------------------------------------------------

                doc.Write(fs);
                doc.Close();
            }
            return Json(1);

        }
        //-------------------------------------------------------------------------------------------


    }
}