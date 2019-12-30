using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;


namespace CaloriesAndroid
{
    [Activity(Label = "Calculating")]
    public class Calculating : Activity
    {
        SQLiteConnection conn;
        int Calories = 0;
        FoodCaloriesTable tempFood;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Counting);
            // Create your application here
            var foodname = FindViewById<EditText>(Resource.Id.FoodName);
            foodname.AfterTextChanged += Foodname_AfterTextChanged;
            var caloriecontent = FindViewById<TextView>(Resource.Id.CalorieContent);
            caloriecontent.Text = foodname.Text;
            var todbpath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), 
                "fooddata.db3");
            var dbfile = File.Create(todbpath);
            Assets.Open("fooddata.sqlite").CopyTo(dbfile);
            conn = new SQLiteConnection(todbpath);
            var count = conn.Table<FoodCaloriesTable>();
            var foodnames = new string[10024];
            var _index = 0;
            foreach(var obj in count)
            {
                foodnames[_index] = obj.FoodName;
                _index += 1;
                //Console.WriteLine(obj.FoodName);
            }

            //var autocomplete = FindViewById<AutoCompleteTextView>(Resource.Id.autocomplete_foodname);

            //autocomplete.Adapter = new ArrayAdapter<string>(this, Resource.Layout.Counting, foodnames);


            // init calculate
            var addButton = FindViewById<Button>(Resource.Id.AddFood);
            addButton.Click += AddButton_Click;


        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            //var foodname = FindViewById<EditText>(Resource.Id.FoodName);

            var quality = FindViewById<EditText>(Resource.Id.Quality);
            Calories += int.Parse( tempFood.Calories) * int.Parse(quality.Text);
            FindViewById<TextView>(Resource.Id.FoodWeight).Text = Calories.ToString();
        }

        private void Foodname_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            var caloriecontent = FindViewById<TextView>(Resource.Id.CalorieContent);
            caloriecontent.Text = "None";

            var foodname = FindViewById<EditText>(Resource.Id.FoodName).Text;
            foreach (var obj in conn.Table<FoodCaloriesTable>())
            {
                if (obj.FoodName == foodname || obj.FoodName.ToLower().StartsWith(foodname.ToLower()))
                {
                    caloriecontent.Text = obj.Calories;
                    tempFood = obj;
                    return;
                }
            }

        }

        private void ExcuteCmdInFile(SQLiteConnection db, string sqlFile)
        {
            //Console.WriteLine(Assets.List("")[2]);
            var foodfile = new StreamReader(Assets.Open(sqlFile));
            string content;
            string init_sqls = "";
            while ((content = foodfile.ReadLine()) != null)
            {
                if (content.TrimStart().StartsWith("--")) continue;
                init_sqls += content.ToString();
            }
            var initSqlList = init_sqls.Split(";");
            for (var i = 0; i < initSqlList.Length - 1; i++)
            {
                initSqlList[i] = initSqlList[i] + ";";
            }
            foreach (var sql in initSqlList)
            {
                try
                {
                    db.Execute(sql);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Sql not excute: ", sql);
                    Console.WriteLine(e.Message);
                }
            }
            var tempResult = db.Query<FoodCaloriesTable>("select * from FoodData");
            Console.WriteLine(db.Query<FoodCaloriesTable>("select * from FoodData"));
        }

        

        private int AddFood(string foodName, TableQuery<FoodCaloriesTable> count)
        {
            foreach (var obj in count)
            {
                if(obj.FoodName == foodName)
                {
                    return int.Parse( obj.Calories);
                }
            }
            return 0;
        }
    }

    [Table("FoodData")]
    public class FoodCaloriesTable
    {
        [PrimaryKey]
        public int DatabaseNumber { get; set ;  }
        public string Calories { get; set; }
        public string FoodName { get; set; }
    }
}