ConfigHelper enables to you easily save and load data from a .config or .xml file by inputting an object with properties to represent the data.

An example configuration class:

    public class MyConfiguration {
        private readonly string _configName;
        private readonly string _configPath;
        public MyConfiguration(string configName, string configPath) {
            _configName = configName;
            _configPath = configPath;
        }
        [IsSetting]
        public string MyString { get; set; }
        [IsSetting]
        public double MyDouble { get; set; }
        [IsSetting]
        public Point[] MyPoints { get; set; }
        public void Save() {
            ConfigSettings.SaveXmlConfig(this, _configName, _configPath);
        }
        public void Load() {
            ConfigSettings.LoadXmlConfig(this, _configName, _configPath);
        }
    }

any properties marked with the attribute `[IsSetting]` will be saved to the desired configuration file, in this case it will be an xml file.

Example usage:

    var config = new MyConfiguration("configuration", Server.MapPath("~/")) { 
        MyString = "TestString",
        MyDouble = 32.53,
        MyPoints = new [] { new Point(0,0), new Point(100,100) }
    };
    config.Save(); //this will have saved the values and created a file called "configuration.xml" in the root directory
    //we empty everything
    config.MyString = "";
    config.MyDouble = 0;
    config.MyPoints = new Points[]{};
    config.Load(); //we then load all the values from the file to show that it works
    