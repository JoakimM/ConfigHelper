ConfigHelper enables to you easily save and load data from a .config or .xml file by inputting an object with properties to represent the data.

An example configuration class:
Using ConfigHelper as of v2 doesn't change too much but is significant:

    public class MyConfiguration {
        //First change is the IConfigSettings interface which has been created
        private readonly IConfigSettings _configSettings;
        private readonly string _configName;
        private readonly string _configPath;

        public MyConfiguration(IConfigSettings configSettings, string configName, string configPath)
        {
            _configSettings = configSettings;
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
    	    //This is the second change and also what might be a bit confusing
    	    //all Save methods has become one method and now relies on
    	    //.Save<T> where T is a class which tells what type of config it is
    	    //in this example we are using a XmlConfig hence .Save<XmlConfig>
    	    //the others are AppConfig, WebConfig and CustomConfig
            _configSettings.Save<XmlConfig>(this, _configName, _configPath);
        }
        public void Load() {
    	    //The same change is true for this .Load<T> method
            _configSettings.Load<XmlConfig>(this, _configName, _configPath);
        }
    }

any properties marked with the attribute `[IsSetting]` will be saved to the desired configuration file, in this case it will be an xml file.

Example usage:

    protected void Page_Load(object sender, EventArgs e)
    {
        IConfigSettings configSettings = new ConfigSettings();
        var tmpString = "TestString";
        var config = new MyConfiguration(configSettings, "configuration", Server.MapPath("~/"))
                         {
                             MyString = tmpString,
                             MyDouble = 29.42,
                             MyPoints = new[] {new Point(3, 2), new Point(45, 78)}
                         };
        config.Save();
        //We reset everything
        config.MyString = "";
        config.MyDouble = 0;
        config.MyPoints = new Point[]{};
        //We load it
        config.Load();
        if (config.MyString == tmpString)
        {
            //we know it worked it if gets here
            var i = 1;
        }
    }