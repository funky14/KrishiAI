namespace KrishiAI.App.Resources.Strings;

public class AppStrings
{
    // Home Page
    public static string KrishiAI => GetString("KrishiAI");
    public static string YourAIFarmingCompanion => GetString("YourAIFarmingCompanion");
    public static string Greeting => GetString("Greeting");
    public static string Farmer => GetString("Farmer");
    public static string SmartFarming => GetString("SmartFarming");
    public static string BetterTomorrow => GetString("BetterTomorrow");
    public static string HowCanIHelp => GetString("HowCanIHelp");
    public static string CropDiseaseDetection => GetString("CropDiseaseDetection");
    public static string CropDiseaseDescription => GetString("CropDiseaseDescription");
    public static string DetectDisease => GetString("DetectDisease");
    public static string VoiceAssistant => GetString("VoiceAssistant");
    public static string VoiceAssistantDescription => GetString("VoiceAssistantDescription");
    public static string AskQuestion => GetString("AskQuestion");
    public static string QuickFeatures => GetString("QuickFeatures");
    public static string Languages => GetString("Languages");
    public static string FarmingTips => GetString("FarmingTips");

    // Greetings
    public static string GoodMorning => GetString("GoodMorning");
    public static string GoodAfternoon => GetString("GoodAfternoon");
    public static string GoodEvening => GetString("GoodEvening");
    public static string GoodNight => GetString("GoodNight");

    // History Page
    public static string Refresh => GetString("Refresh");
    public static string ClearAllHistory => GetString("ClearAllHistory");

    // Settings Page
    public static string Settings => GetString("Settings");
    public static string Preferences => GetString("Preferences");
    public static string DefaultLanguage => GetString("DefaultLanguage");
    public static string LanguageHelpText => GetString("LanguageHelpText");
    public static string SaveHistory => GetString("SaveHistory");
    public static string SaveHistoryDescription => GetString("SaveHistoryDescription");
    public static string AutoPlayResponses => GetString("AutoPlayResponses");
    public static string AutoPlayResponsesDescription => GetString("AutoPlayResponsesDescription");
    public static string SaveSettings => GetString("SaveSettings");
    public static string DataManagement => GetString("DataManagement");
    public static string ClearCache => GetString("ClearCache");
    public static string ClearHistory => GetString("ClearHistory");
    public static string About => GetString("About");
    public static string AboutDescription => GetString("AboutDescription");
    public static string Features => GetString("Features");
    public static string Feature1 => GetString("Feature1");
    public static string Feature2 => GetString("Feature2");
    public static string Feature3 => GetString("Feature3");
    public static string Feature4 => GetString("Feature4");
    public static string Copyright => GetString("Copyright");

    // Common
    public static string Success => GetString("Success");
    public static string Error => GetString("Error");
    public static string Yes => GetString("Yes");
    public static string No => GetString("No");
    public static string OK => GetString("OK");
    public static string Cancel => GetString("Cancel");
    public static string Home => GetString("Home");
    public static string History => GetString("History");

    // About Section Additions
    public static string AppDescription => GetString("AppDescription");
    public static string FeaturesTitle => GetString("FeaturesTitle");
    public static string Feature1Text => GetString("Feature1Text");
    public static string Feature2Text => GetString("Feature2Text");
    public static string Feature3Text => GetString("Feature3Text");
    public static string Feature4Text => GetString("Feature4Text");
    public static string CopyrightText => GetString("CopyrightText");

    // Messages
    public static string SettingsSavedSuccess => GetString("SettingsSavedSuccess");
    public static string ClearCacheConfirm => GetString("ClearCacheConfirm");
    public static string ClearHistoryConfirm => GetString("ClearHistoryConfirm");
    public static string CacheClearedSuccess => GetString("CacheClearedSuccess");
    public static string HistoryClearedSuccess => GetString("HistoryClearedSuccess");
    public static string Notifications => GetString("Notifications");
    public static string ClearNotifications => GetString("ClearNotifications");
    public static string ChooseLanguage => GetString("ChooseLanguage");

    // Crop Disease Page
    public static string NoImageSelected => GetString("NoImageSelected");
    public static string CaptureOrSelectImage => GetString("CaptureOrSelectImage");
    public static string Capture => GetString("Capture");
    public static string Gallery => GetString("Gallery");
    public static string AnalyzeDisease => GetString("AnalyzeDisease");
    public static string AnalyzingImage => GetString("AnalyzingImage");
    public static string DetectionResults => GetString("DetectionResults");
    public static string Confidence => GetString("Confidence");
    public static string Severity => GetString("Severity");
    public static string TreatmentRecommendations => GetString("TreatmentRecommendations");
    public static string OrganicTreatment => GetString("OrganicTreatment");
    public static string ChemicalTreatment => GetString("ChemicalTreatment");
    public static string PreventionTips => GetString("PreventionTips");
    public static string NewAnalysis => GetString("NewAnalysis");

    // Voice Assistant Page
    public static string SelectLanguage => GetString("SelectLanguage");
    public static string Listening => GetString("Listening");
    public static string TapToSpeak => GetString("TapToSpeak");
    public static string ClearChat => GetString("ClearChat");

    // History Page
    public static string DiseaseDetectionHistory => GetString("DiseaseDetectionHistory");
    public static string NoHistoryYet => GetString("NoHistoryYet");
    public static string StartAnalyzingCrops => GetString("StartAnalyzingCrops");
    public static string ViewDetails => GetString("ViewDetails");

    // Farmer Tips Page
    public static string FarmerTipsTitle => GetString("FarmerTipsTitle");

    // Public method for dynamic access
    public static string GetLocalizedString(string key) => GetString(key);

    private static string GetString(string key)
    {
        var currentLanguage = System.Globalization.CultureInfo.CurrentUICulture.Name;

        var translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["en-US"] = new()
            {
                ["Settings"] = "Settings",
                ["Preferences"] = "Preferences",
                ["DefaultLanguage"] = "Default Language",
                ["LanguageHelpText"] = "Language will be applied throughout the app",
                ["SaveHistory"] = "Save History",
                ["SaveHistoryDescription"] = "Store detection results locally",
                ["AutoPlayResponses"] = "Auto Play Responses",
                ["AutoPlayResponsesDescription"] = "Automatically play voice responses",
                ["SaveSettings"] = "Save Settings",
                ["DataManagement"] = "Data Management",
                ["ClearCache"] = "🗑️ Clear Cache",
                ["ClearHistory"] = "🗑️ Clear History",
                ["About"] = "About",
                ["Success"] = "Success",
                ["Error"] = "Error",
                ["Yes"] = "Yes",
                ["No"] = "No",
                ["OK"] = "OK",
                ["Cancel"] = "Cancel",
                ["SettingsSavedSuccess"] = "Settings saved successfully. The language change will be applied throughout the app.",
                ["ClearCacheConfirm"] = "This will delete all cached images. Continue?",
                ["ClearHistoryConfirm"] = "This will delete all detection history. Continue?",
                ["CacheClearedSuccess"] = "Cache cleared successfully",
                ["HistoryClearedSuccess"] = "History cleared successfully",
                ["Home"] = "Home",
                ["Welcome"] = "Welcome",
                ["CropDiseaseDetection"] = "Crop Disease Detection",
                ["VoiceAssistant"] = "Voice Assistant",
                ["History"] = "History",
                ["Farmer"] = "Farmer",
                ["KrishiAI"] = "KrishiAI",
                ["YourAIFarmingCompanion"] = "Your AI Farming Companion",
                ["SmartFarming"] = "Smart Farming for",
                ["BetterTomorrow"] = "Better Tomorrow",
                ["HowCanIHelp"] = "How can I help you today?",
                ["QuickFeatures"] = "Quick Features",
                ["Languages"] = "8 Languages",
                ["FarmingTips"] = "Farming Tips",
                ["DetectDisease"] = "Detect Disease",
                ["AskQuestion"] = "Ask a Question",
                ["Notifications"] = "Notifications",
                ["ClearNotifications"] = "Clear Notifications",
                ["ChooseLanguage"] = "Choose Language",

                // Descriptions
                ["AppDescription"] = "KrishiAI is your AI-powered farming assistant that helps detect crop diseases and provides agricultural advice in multiple Indian languages.",
                ["FeaturesTitle"] = "Features:",
                ["Feature1Text"] = "• On-device AI disease detection",
                ["Feature2Text"] = "• Multilingual voice assistant",
                ["Feature3Text"] = "• Offline-first architecture",
                ["Feature4Text"] = "• Treatment recommendations",
                ["CopyrightText"] = "© 2026 KrishiAI Team",
                ["CropDiseaseDescription"] = "Take a photo of your crop and get instant disease identification with treatment recommendations.",
                ["VoiceAssistantDescription"] = "Ask farming questions in your language and get AI-powered advice instantly.",

                // Additional fields
                ["NoImageSelected"] = "No image selected",
                ["CaptureOrSelectImage"] = "Capture or select a crop image",
                ["Capture"] = "Capture",
                ["Gallery"] = "Gallery",
                ["AnalyzeDisease"] = "Analyze Disease",
                ["AnalyzingImage"] = "Analyzing crop image...",
                ["DetectionResults"] = "Detection Results",
                ["Confidence"] = "Confidence",
                ["Severity"] = "Severity",
                ["TreatmentRecommendations"] = "Treatment Recommendations",
                ["OrganicTreatment"] = "Organic Treatment",
                ["ChemicalTreatment"] = "Chemical Treatment",
                ["PreventionTips"] = "Prevention Tips",
                ["NewAnalysis"] = "New Analysis",
                ["SelectLanguage"] = "Select Language",
                ["Listening"] = "Listening...",
                ["TapToSpeak"] = "Tap to Speak",
                ["ClearChat"] = "Clear Chat",
                ["DiseaseDetectionHistory"] = "Disease Detection History",
                ["NoHistoryYet"] = "No history yet",
                ["StartAnalyzingCrops"] = "Start analyzing crops to see history here",
                ["ViewDetails"] = "View Details",
                ["FarmerTipsTitle"] = "Farmer Tips",

                // Greetings
                ["GoodMorning"] = "Good Morning",
                ["GoodAfternoon"] = "Good Afternoon",
                ["GoodEvening"] = "Good Evening",
                ["GoodNight"] = "Good Night",

                // History Page
                ["Refresh"] = "Refresh",
                ["ClearAllHistory"] = "Clear All History"
            },
            ["hi-IN"] = new()
            {
                ["Settings"] = "सेटिंग्स",
                ["Preferences"] = "प्राथमिकताएं",
                ["DefaultLanguage"] = "डिफ़ॉल्ट भाषा",
                ["LanguageHelpText"] = "भाषा पूरे ऐप में लागू होगी",
                ["SaveHistory"] = "इतिहास सहेजें",
                ["SaveHistoryDescription"] = "पहचान परिणामों को स्थानीय रूप से संग्रहीत करें",
                ["AutoPlayResponses"] = "स्वतः प्रतिक्रिया चलाएं",
                ["AutoPlayResponsesDescription"] = "स्वचालित रूप से आवाज प्रतिक्रिया चलाएं",
                ["SaveSettings"] = "सेटिंग्स सहेजें",
                ["DataManagement"] = "डेटा प्रबंधन",
                ["ClearCache"] = "🗑️ कैश साफ़ करें",
                ["ClearHistory"] = "🗑️ इतिहास साफ़ करें",
                ["About"] = "के बारे में",
                ["Success"] = "सफलता",
                ["Error"] = "त्रुटि",
                ["Yes"] = "हाँ",
                ["No"] = "नहीं",
                ["OK"] = "ठीक है",
                ["Cancel"] = "रद्द करें",
                ["SettingsSavedSuccess"] = "सेटिंग्स सफलतापूर्वक सहेजी गईं। भाषा परिवर्तन पूरे ऐप में लागू होगा।",
                ["ClearCacheConfirm"] = "यह सभी कैश की गई छवियों को हटा देगा। जारी रखें?",
                ["ClearHistoryConfirm"] = "यह सभी पहचान इतिहास को हटा देगा। जारी रखें?",
                ["CacheClearedSuccess"] = "कैश सफलतापूर्वक साफ़ किया गया",
                ["HistoryClearedSuccess"] = "इतिहास सफलतापूर्वक साफ़ किया गया",
                ["Home"] = "होम",
                ["Welcome"] = "स्वागत है",
                ["CropDiseaseDetection"] = "फसल रोग का पता लगाना",
                ["VoiceAssistant"] = "आवाज सहायक",
                ["History"] = "इतिहास",
                ["Farmer"] = "किसान",
                ["KrishiAI"] = "कृषि एआई",
                ["YourAIFarmingCompanion"] = "आपका एआई खेती साथी",
                ["SmartFarming"] = "स्मार्ट खेती के लिए",
                ["BetterTomorrow"] = "बेहतर कल",
                ["HowCanIHelp"] = "आज मैं आपकी कैसे मदद कर सकता हूं?",
                ["QuickFeatures"] = "त्वरित सुविधाएँ",
                ["Languages"] = "8 भाषाएँ",
                ["FarmingTips"] = "खेती टिप्स",
                ["DetectDisease"] = "रोग का पता लगाएं",
                ["AskQuestion"] = "प्रश्न पूछें",
                ["Notifications"] = "सूचनाएं",
                ["ClearNotifications"] = "सूचनाएं साफ़ करें",
                ["ChooseLanguage"] = "भाषा चुनें",

                // Descriptions
                ["AppDescription"] = "कृषिAI आपका AI-संचालित कृषि सहायक है जो फसल रोगों का पता लगाने और कई भारतीय भाषाओं में कृषि सलाह प्रदान करने में मदद करता है।",
                ["FeaturesTitle"] = "विशेषताएं:",
                ["Feature1Text"] = "• ऑन-डिवाइस AI रोग पहचान",
                ["Feature2Text"] = "• बहुभाषी आवाज सहायक",
                ["Feature3Text"] = "• ऑफ़लाइन-फर्स्ट आर्किटेक्चर",
                ["Feature4Text"] = "• उपचार सिफारिशें",
                ["CopyrightText"] = "© 2026 कृषिAI टीम",
                ["CropDiseaseDescription"] = "अपनी फसल की फोटो लें और उपचार सिफारिशों के साथ तुरंत रोग की पहचान करें।",
                ["VoiceAssistantDescription"] = "अपनी भाषा में खेती के सवाल पूछें और तुरंत AI-संचालित सलाह प्राप्त करें।",

                // Additional fields
                ["NoImageSelected"] = "कोई छवि चयनित नहीं",
                ["CaptureOrSelectImage"] = "फसल की छवि कैप्चर या चुनें",
                ["Capture"] = "कैप्चर",
                ["Gallery"] = "गैलरी",
                ["AnalyzeDisease"] = "रोग का विश्लेषण करें",
                ["AnalyzingImage"] = "फसल छवि का विश्लेषण कर रहे हैं...",
                ["DetectionResults"] = "पहचान परिणाम",
                ["Confidence"] = "विश्वास",
                ["Severity"] = "गंभीरता",
                ["TreatmentRecommendations"] = "उपचार सिफारिशें",
                ["OrganicTreatment"] = "जैविक उपचार",
                ["ChemicalTreatment"] = "रासायनिक उपचार",
                ["PreventionTips"] = "रोकथाम युक्तियाँ",
                ["NewAnalysis"] = "नया विश्लेषण",
                ["SelectLanguage"] = "भाषा चुनें",
                ["Listening"] = "सुन रहे हैं...",
                ["TapToSpeak"] = "बोलने के लिए टैप करें",
                ["ClearChat"] = "चैट साफ़ करें",
                ["DiseaseDetectionHistory"] = "रोग पहचान इतिहास",
                ["NoHistoryYet"] = "अभी तक कोई इतिहास नहीं",
                ["StartAnalyzingCrops"] = "यहां इतिहास देखने के लिए फसलों का विश्लेषण शुरू करें",
                ["ViewDetails"] = "विवरण देखें",
                ["FarmerTipsTitle"] = "किसान टिप्स",

                // Greetings
                ["GoodMorning"] = "सुप्रभात",
                ["GoodAfternoon"] = "शुभ दोपहर",
                ["GoodEvening"] = "शुभ संध्या",
                ["GoodNight"] = "शुभ रात्रि",

                // History Page
                ["Refresh"] = "रीफ्रेश करें",
                ["ClearAllHistory"] = "सभी इतिहास साफ़ करें"
            },
            ["mr-IN"] = new()
            {
                ["Settings"] = "सेटिंग्ज",
                ["Preferences"] = "प्राधान्ये",
                ["DefaultLanguage"] = "डीफॉल्ट भाषा",
                ["LanguageHelpText"] = "भाषा संपूर्ण अॅपमध्ये लागू होईल",
                ["SaveHistory"] = "इतिहास जतन करा",
                ["SaveHistoryDescription"] = "शोध परिणाम स्थानिक पातळीवर साठवा",
                ["AutoPlayResponses"] = "स्वयं प्रतिसाद प्ले करा",
                ["AutoPlayResponsesDescription"] = "आवाज प्रतिसाद स्वयंचलितपणे प्ले करा",
                ["SaveSettings"] = "सेटिंग्ज जतन करा",
                ["DataManagement"] = "डेटा व्यवस्थापन",
                ["ClearCache"] = "🗑️ कॅशे साफ करा",
                ["ClearHistory"] = "🗑️ इतिहास साफ करा",
                ["About"] = "बद्दल",
                ["Success"] = "यश",
                ["Error"] = "त्रुटी",
                ["Yes"] = "होय",
                ["No"] = "नाही",
                ["OK"] = "ठीक आहे",
                ["Cancel"] = "रद्द करा",
                ["SettingsSavedSuccess"] = "सेटिंग्ज यशस्वीरित्या जतन केल्या. भाषा बदल संपूर्ण अॅपमध्ये लागू होईल.",
                ["ClearCacheConfirm"] = "हे सर्व कॅशे केलेल्या प्रतिमा हटवेल. सुरू ठेवायचे?",
                ["ClearHistoryConfirm"] = "हे सर्व शोध इतिहास हटवेल. सुरू ठेवायचे?",
                ["CacheClearedSuccess"] = "कॅशे यशस्वीरित्या साफ केले",
                ["HistoryClearedSuccess"] = "इतिहास यशस्वीरित्या साफ केला",
                ["Home"] = "मुख्यपृष्ठ",
                ["Welcome"] = "स्वागत आहे",
                ["CropDiseaseDetection"] = "पीक रोग शोध",
                ["VoiceAssistant"] = "आवाज सहाय्यक",
                ["History"] = "इतिहास",
                ["Farmer"] = "शेतकरी",
                ["KrishiAI"] = "कृषि एआय",
                ["YourAIFarmingCompanion"] = "तुमचा एआय शेती साथी",
                ["SmartFarming"] = "स्मार्ट शेती साठी",
                ["BetterTomorrow"] = "चांगल्या उद्याचा",
                ["HowCanIHelp"] = "आज मी तुम्हाला कशी मदत करू?",
                ["QuickFeatures"] = "जलद वैशिष्ट्ये",
                ["Languages"] = "८ भाषा",
                ["FarmingTips"] = "शेती टिप्स",
                ["DetectDisease"] = "रोग शोधा",
                ["AskQuestion"] = "प्रश्न विचारा",
                ["Notifications"] = "सूचना",
                ["ClearNotifications"] = "सूचना साफ करा",
                ["ChooseLanguage"] = "भाषा निवडा",

                // Descriptions
                ["AppDescription"] = "कृषिAI तुमचा AI-चालित शेती सहाय्यक आहे जो पीक रोगांचा शोध घेण्यास आणि अनेक भारतीय भाषांमध्ये कृषी सल्ला प्रदान करण्यास मदत करतो।",
                ["FeaturesTitle"] = "वैशिष्ट्ये:",
                ["Feature1Text"] = "• ऑन-डिव्हाइस AI रोग शोध",
                ["Feature2Text"] = "• बहुभाषिक आवाज सहाय्यक",
                ["Feature3Text"] = "• ऑफलाइन-प्रथम आर्किटेक्चर",
                ["Feature4Text"] = "• उपचार शिफारशी",
                ["CopyrightText"] = "© 2026 कृषिAI टीम",
                ["CropDiseaseDescription"] = "तुमच्या पिकाचा फोटो घ्या आणि उपचार शिफारशींसह त्वरित रोग ओळख मिळवा।",
                ["VoiceAssistantDescription"] = "तुमच्या भाषेत शेतीचे प्रश्न विचारा आणि त्वरित AI-चालित सल्ला मिळवा।",

                // Additional Crop Disease fields
                ["NoImageSelected"] = "कोणतीही प्रतिमा निवडलेली नाही",
                ["CaptureOrSelectImage"] = "पीक प्रतिमा कॅप्चर करा किंवा निवडा",
                ["Capture"] = "कॅप्चर",
                ["Gallery"] = "गॅलरी",
                ["AnalyzeDisease"] = "रोगाचे विश्लेषण करा",
                ["AnalyzingImage"] = "पीक प्रतिमेचे विश्लेषण करत आहे...",
                ["DetectionResults"] = "शोध परिणाम",
                ["Confidence"] = "विश्वास",
                ["Severity"] = "तीव्रता",
                ["TreatmentRecommendations"] = "उपचार शिफारशी",
                ["OrganicTreatment"] = "सेंद्रिय उपचार",
                ["ChemicalTreatment"] = "रासायनिक उपचार",
                ["PreventionTips"] = "प्रतिबंध टिप्स",
                ["NewAnalysis"] = "नवीन विश्लेषण",
                ["SelectLanguage"] = "भाषा निवडा",
                ["Listening"] = "ऐकत आहे...",
                ["TapToSpeak"] = "बोलण्यासाठी टॅप करा",
                ["ClearChat"] = "चॅट साफ करा",
                ["DiseaseDetectionHistory"] = "रोग शोध इतिहास",
                ["NoHistoryYet"] = "अद्याप कोणताही इतिहास नाही",
                ["StartAnalyzingCrops"] = "येथे इतिहास पाहण्यासाठी पिकांचे विश्लेषण सुरू करा",
                ["ViewDetails"] = "तपशील पहा",
                ["FarmerTipsTitle"] = "शेतकरी टिप्स",

                // Greetings
                ["GoodMorning"] = "सुप्रभात",
                ["GoodAfternoon"] = "शुभ दुपार",
                ["GoodEvening"] = "शुभ संध्याकाळ",
                ["GoodNight"] = "शुभ रात्री",

                // History Page
                ["Refresh"] = "रीफ्रेश करा",
                ["ClearAllHistory"] = "सर्व इतिहास साफ करा"
            },
            ["ta-IN"] = new()
            {
                ["Settings"] = "அமைப்புகள்",
                ["Preferences"] = "விருப்பத்தேர்வுகள்",
                ["DefaultLanguage"] = "இயல்புநிலை மொழி",
                ["LanguageHelpText"] = "மொழி முழு பயன்பாட்டிலும் பயன்படுத்தப்படும்",
                ["SaveHistory"] = "வரலாற்றைச் சேமிக்கவும்",
                ["SaveHistoryDescription"] = "கண்டறிதல் முடிவுகளை உள்ளூரில் சேமிக்கவும்",
                ["AutoPlayResponses"] = "தானியங்கு பதில்கள்",
                ["AutoPlayResponsesDescription"] = "குரல் பதில்களை தானாகவே இயக்கவும்",
                ["SaveSettings"] = "அமைப்புகளைச் சேமிக்கவும்",
                ["DataManagement"] = "தரவு மேலாண்மை",
                ["ClearCache"] = "🗑️ தேக்ககத்தை அழிக்கவும்",
                ["ClearHistory"] = "🗑️ வரலாற்றை அழிக்கவும்",
                ["About"] = "பற்றி",
                ["Success"] = "வெற்றி",
                ["Error"] = "பிழை",
                ["Yes"] = "ஆம்",
                ["No"] = "இல்லை",
                ["OK"] = "சரி",
                ["Cancel"] = "ரத்து செய்",
                ["SettingsSavedSuccess"] = "அமைப்புகள் வெற்றிகரமாகச் சேமிக்கப்பட்டன. மொழி மாற்றம் முழு பயன்பாட்டிலும் பயன்படுத்தப்படும்.",
                ["ClearCacheConfirm"] = "இது அனைத்து தேக்கக படங்களையும் நீக்கும். தொடரவா?",
                ["ClearHistoryConfirm"] = "இது அனைத்து கண்டறிதல் வரலாற்றையும் நீக்கும். தொடரவா?",
                ["CacheClearedSuccess"] = "தேக்ககம் வெற்றிகரமாக அழிக்கப்பட்டது",
                ["HistoryClearedSuccess"] = "வரலாறு வெற்றிகரமாக அழிக்கப்பட்டது",
                ["Home"] = "முகப்பு",
                ["Welcome"] = "வரவேற்கிறோம்",
                ["CropDiseaseDetection"] = "பயிர் நோய் கண்டறிதல்",
                ["VoiceAssistant"] = "குரல் உதவியாளர்",
                ["History"] = "வரலாறு",
                ["Farmer"] = "விவசாயி",
                ["KrishiAI"] = "கிருஷி ஏஐ",
                ["YourAIFarmingCompanion"] = "உங்கள் ஏஐ விவசாய துணை",
                ["SmartFarming"] = "ஸ்மார்ட் விவசாயத்திற்கு",
                ["BetterTomorrow"] = "சிறந்த நாளைக்கு",
                ["HowCanIHelp"] = "இன்று நான் உங்களுக்கு எப்படி உதவலாம்?",
                ["QuickFeatures"] = "விரைவு அம்சங்கள்",
                ["Languages"] = "8 மொழிகள்",
                ["FarmingTips"] = "விவசாய உதவிக்குறிப்புகள்",
                ["DetectDisease"] = "நோயைக் கண்டறியவும்",
                ["AskQuestion"] = "கேள்வி கேளுங்கள்",
                ["Notifications"] = "அறிவிப்புகள்",
                ["ClearNotifications"] = "அறிவிப்புகளை அழி",
                ["ChooseLanguage"] = "மொழியைத் தேர்வு செய்யவும்",

                // Descriptions
                ["AppDescription"] = "கிருஷி ஏஐ உங்கள் ஏஐ-இயங்கும் விவசாய உதவியாளர் ஆகும், இது பயிர் நோய்களைக் கண்டறிய உதவுகிறது மற்றும் பல இந்திய மொழிகளில் விவசாய ஆலோசனையை வழங்குகிறது.",
                ["FeaturesTitle"] = "அம்சங்கள்:",
                ["Feature1Text"] = "• சாதனத்தில் ஏஐ நோய் கண்டறிதல்",
                ["Feature2Text"] = "• பல்மொழி குரல் உதவியாளர்",
                ["Feature3Text"] = "• ஆஃப்லைன்-முதல் கட்டமைப்பு",
                ["Feature4Text"] = "• சிகிச்சை பரிந்துரைகள்",
                ["CopyrightText"] = "© 2026 கிருஷி ஏஐ குழு",
                ["CropDiseaseDescription"] = "உங்கள் பயிரின் புகைப்படத்தை எடுத்து சிகிச்சை பரிந்துரைகளுடன் உடனடி நோய் அடையாளத்தைப் பெறுங்கள்.",
                ["VoiceAssistantDescription"] = "உங்கள் மொழியில் விவசாய கேள்விகளைக் கேளுங்கள் மற்றும் உடனடியாக ஏஐ-இயக்கப்பட்ட ஆலோசனையைப் பெறுங்கள்",

                // Additional Crop Disease fields
                ["NoImageSelected"] = "படம் தேர்ந்தெடுக்கப்படவில்லை",
                ["CaptureOrSelectImage"] = "பயிர் படத்தை எடுக்கவும் அல்லது தேர்வு செய்யவும்",
                ["Capture"] = "எடு",
                ["Gallery"] = "கேலரி",
                ["AnalyzeDisease"] = "நோயை பகுப்பாய்வு செய்",
                ["AnalyzingImage"] = "பயிர் படத்தை பகுப்பாய்வு செய்கிறது...",
                ["DetectionResults"] = "கண்டறிதல் முடிவுகள்",
                ["Confidence"] = "நம்பிக்கை",
                ["Severity"] = "தீவிரம்",
                ["TreatmentRecommendations"] = "சிகிச்சை பரிந்துரைகள்",
                ["OrganicTreatment"] = "இயற்கை சிகிச்சை",
                ["ChemicalTreatment"] = "இரசாயன சிகிச்சை",
                ["PreventionTips"] = "தடுப்பு குறிப்புகள்",
                ["NewAnalysis"] = "புதிய பகுப்பாய்வு",
                ["SelectLanguage"] = "மொழியைத் தேர்வு செய்யவும்",
                ["Listening"] = "கேட்கிறது...",
                ["TapToSpeak"] = "பேச தட்டவும்",
                ["ClearChat"] = "அரட்டையை அழி",
                ["DiseaseDetectionHistory"] = "நோய் கண்டறிதல் வரலாறு",
                ["NoHistoryYet"] = "இன்னும் வரலாறு இல்லை",
                ["StartAnalyzingCrops"] = "இங்கே வரலாற்றைக் காண பயிர்களை பகுப்பாய்வு செய்யத் தொடங்குங்கள்",
                ["ViewDetails"] = "விவரங்களைக் காண்க",
                ["FarmerTipsTitle"] = "விவசாயி குறிப்புகள்",

                // Greetings
                ["GoodMorning"] = "காலை வணக்கம்",
                ["GoodAfternoon"] = "மதிய வணக்கம்",
                ["GoodEvening"] = "மாலை வணக்கம்",
                ["GoodNight"] = "இனிய இரவு",

                // History Page
                ["Refresh"] = "புதுப்பிக்கவும்",
                ["ClearAllHistory"] = "அனைத்து வரலாற்றையும் அழி"
            },
            ["te-IN"] = new()
            {
                ["Settings"] = "సెట్టింగ్‌లు",
                ["Preferences"] = "ప్రాధాన్యతలు",
                ["DefaultLanguage"] = "డిఫాల్ట్ భాష",
                ["LanguageHelpText"] = "భాష అప్లికేషన్ అంతటా వర్తించబడుతుంది",
                ["SaveHistory"] = "చరిత్రను సేవ్ చేయండి",
                ["SaveHistoryDescription"] = "డిటెక్షన్ ఫలితాలను స్థానికంగా నిల్వ చేయండి",
                ["AutoPlayResponses"] = "ఆటో ప్లే రెస్పాన్స్‌లు",
                ["AutoPlayResponsesDescription"] = "వాయిస్ రెస్పాన్స్‌లను స్వయంచాలకంగా ప్లే చేయండి",
                ["SaveSettings"] = "సెట్టింగ్‌లను సేవ్ చేయండి",
                ["DataManagement"] = "డేటా నిర్వహణ",
                ["ClearCache"] = "🗑️ కాష్ క్లియర్ చేయండి",
                ["ClearHistory"] = "🗑️ చరిత్రను క్లియర్ చేయండి",
                ["About"] = "గురించి",
                ["Success"] = "విజయం",
                ["Error"] = "లోపం",
                ["Yes"] = "అవును",
                ["No"] = "కాదు",
                ["OK"] = "సరే",
                ["Cancel"] = "రద్దు చేయండి",
                ["SettingsSavedSuccess"] = "సెట్టింగ్‌లు విజయవంతంగా సేవ్ చేయబడ్డాయి. భాషా మార్పు అప్లికేషన్ అంతటా వర్తించబడుతుంది.",
                ["ClearCacheConfirm"] = "ఇది అన్ని కాష్ చేయబడిన చిత్రాలను తొలగిస్తుంది. కొనసాగించాలా?",
                ["ClearHistoryConfirm"] = "ఇది అన్ని డిటెక్షన్ చరిత్రను తొలగిస్తుంది. కొనసాగించాలా?",
                ["CacheClearedSuccess"] = "కాష్ విజయవంతంగా క్లియర్ చేయబడింది",
                ["HistoryClearedSuccess"] = "చరిత్ర విజయవంతంగా క్లియర్ చేయబడింది",
                ["Home"] = "హోమ్",
                ["Welcome"] = "స్వాగతం",
                ["CropDiseaseDetection"] = "పంట వ్యాధి గుర్తింపు",
                ["VoiceAssistant"] = "వాయిస్ అసిస్టెంట్",
                ["History"] = "చరిత్ర",
                ["Farmer"] = "రైతు",
                ["KrishiAI"] = "కృషి ఏఐ",
                ["YourAIFarmingCompanion"] = "మీ ఏఐ వ్యవసాయ సహచరుడు",
                ["SmartFarming"] = "స్మార్ట్ వ్యవసాయం కోసం",
                ["BetterTomorrow"] = "మెరుగైన రేపు కోసం",
                ["HowCanIHelp"] = "ఈ రోజు నేను మీకు ఎలా సహాయం చేయగలను?",
                ["QuickFeatures"] = "త్వరిత ఫీచర్లు",
                ["Languages"] = "8 భాషలు",
                ["FarmingTips"] = "వ్యవసాయ చిట్కాలు",
                ["DetectDisease"] = "వ్యాధిని గుర్తించండి",
                ["AskQuestion"] = "ప్రశ్న అడగండి",
                ["Notifications"] = "నోటిఫికేషన్‌లు",
                ["ClearNotifications"] = "నోటిఫికేషన్‌లను క్లియర్ చేయండి",
                ["ChooseLanguage"] = "భాషను ఎంచుకోండి",

                // Descriptions
                ["AppDescription"] = "కృషి ఏఐ మీ ఏఐ-శక్తితో కూడిన వ్యవసాయ సహాయకుడు, ఇది పంట వ్యాధులను గుర్తించడంలో సహాయపడుతుంది మరియు అనేక భారతీయ భాషలలో వ్యవసాయ సలహాలను అందిస్తుంది.",
                ["FeaturesTitle"] = "ఫీచర్లు:",
                ["Feature1Text"] = "• ఆన్-డివైస్ ఏఐ వ్యాధి గుర్తింపు",
                ["Feature2Text"] = "• బహుభాషా వాయిస్ అసిస్టెంట్",
                ["Feature3Text"] = "• ఆఫ్‌లైన్-ఫస్ట్ ఆర్కిటెక్చర్",
                ["Feature4Text"] = "• చికిత్సా సిఫార్సులు",
                ["CopyrightText"] = "© 2026 కృషి ఏఐ టీమ్",
                ["CropDiseaseDescription"] = "మీ పంట ఫోటోను తీసుకోండి మరియు చికిత్స సిఫార్సులతో తక్షణ వ్యాధి గుర్తింపును పొందండి.",
                ["VoiceAssistantDescription"] = "మీ భాషలో వ్యవసాయ ప్రశ్నలను అడగండి మరియు తక్షణమే ఏఐ-శక్తితో కూడిన సలహాను పొందండి",

                // Additional Crop Disease fields
                ["NoImageSelected"] = "చిత్రం ఎంపిక చేయబడలేదు",
                ["CaptureOrSelectImage"] = "పంట చిత్రాన్ని క్యాప్చర్ చేయండి లేదా ఎంచుకోండి",
                ["Capture"] = "క్యాప్చర్",
                ["Gallery"] = "గ్యాలరీ",
                ["AnalyzeDisease"] = "వ్యాధిని విశ్లేషించండి",
                ["AnalyzingImage"] = "పంట చిత్రాన్ని విశ్లేషిస్తోంది...",
                ["DetectionResults"] = "గుర్తింపు ఫలితాలు",
                ["Confidence"] = "విశ్వాసం",
                ["Severity"] = "తీవ్రత",
                ["TreatmentRecommendations"] = "చికిత్సా సిఫార్సులు",
                ["OrganicTreatment"] = "సేంద్రీయ చికిత్స",
                ["ChemicalTreatment"] = "రసాయన చికిత్స",
                ["PreventionTips"] = "నివారణ చిట్కాలు",
                ["NewAnalysis"] = "కొత్త విశ్లేషణ",
                ["SelectLanguage"] = "భాషను ఎంచుకోండి",
                ["Listening"] = "వింటోంది...",
                ["TapToSpeak"] = "మాట్లాడటానికి ట్యాప్ చేయండి",
                ["ClearChat"] = "చాట్‌ను క్లియర్ చేయండి",
                ["DiseaseDetectionHistory"] = "వ్యాధి గుర్తింపు చరిత్ర",
                ["NoHistoryYet"] = "ఇంకా చరిత్ర లేదు",
                ["StartAnalyzingCrops"] = "ఇక్కడ చరిత్రను చూడటానికి పంటలను విశ్లేషించడం ప్రారంభించండి",
                ["ViewDetails"] = "వివరాలను చూడండి",
                ["FarmerTipsTitle"] = "రైతు చిట్కాలు",

                // Greetings
                ["GoodMorning"] = "శుభోదయం",
                ["GoodAfternoon"] = "శుభ మద్యాహ్నం",
                ["GoodEvening"] = "శుభ సాయంత్రం",
                ["GoodNight"] = "శుభ రాత్రి",

                // History Page
                ["Refresh"] = "రిఫ్రెష్ చేయండి",
                ["ClearAllHistory"] = "మొత్తం చరిత్రను క్లియర్ చేయండి"
            },
            ["pa-IN"] = new()
            {
                ["Settings"] = "ਸੈਟਿੰਗਜ਼",
                ["Preferences"] = "ਤਰਜੀਹਾਂ",
                ["DefaultLanguage"] = "ਡਿਫੌਲਟ ਭਾਸ਼ਾ",
                ["LanguageHelpText"] = "ਭਾਸ਼ਾ ਪੂਰੀ ਐਪ ਵਿੱਚ ਲਾਗੂ ਹੋਵੇਗੀ",
                ["SaveHistory"] = "ਇਤਿਹਾਸ ਸੁਰੱਖਿਅਤ ਕਰੋ",
                ["SaveHistoryDescription"] = "ਖੋਜ ਨਤੀਜਿਆਂ ਨੂੰ ਸਥਾਨਕ ਤੌਰ 'ਤੇ ਸਟੋਰ ਕਰੋ",
                ["AutoPlayResponses"] = "ਆਟੋ ਪਲੇ ਜਵਾਬ",
                ["AutoPlayResponsesDescription"] = "ਆਵਾਜ਼ ਜਵਾਬਾਂ ਨੂੰ ਆਪਣੇ ਆਪ ਚਲਾਓ",
                ["SaveSettings"] = "ਸੈਟਿੰਗਜ਼ ਸੁਰੱਖਿਅਤ ਕਰੋ",
                ["DataManagement"] = "ਡਾਟਾ ਪ੍ਰਬੰਧਨ",
                ["ClearCache"] = "🗑️ ਕੈਸ਼ ਸਾਫ਼ ਕਰੋ",
                ["ClearHistory"] = "🗑️ ਇਤਿਹਾਸ ਸਾਫ਼ ਕਰੋ",
                ["About"] = "ਬਾਰੇ",
                ["Success"] = "ਸਫਲਤਾ",
                ["Error"] = "ਗਲਤੀ",
                ["Yes"] = "ਹਾਂ",
                ["No"] = "ਨਹੀਂ",
                ["OK"] = "ਠੀਕ ਹੈ",
                ["Cancel"] = "ਰੱਦ ਕਰੋ",
                ["SettingsSavedSuccess"] = "ਸੈਟਿੰਗਜ਼ ਸਫਲਤਾਪੂਰਵਕ ਸੁਰੱਖਿਅਤ ਹੋ ਗਈਆਂ। ਭਾਸ਼ਾ ਬਦਲਾਅ ਪੂਰੀ ਐਪ ਵਿੱਚ ਲਾਗੂ ਹੋਵੇਗਾ।",
                ["ClearCacheConfirm"] = "ਇਹ ਸਾਰੀਆਂ ਕੈਸ਼ਡ ਤਸਵੀਰਾਂ ਨੂੰ ਮਿਟਾ ਦੇਵੇਗਾ। ਜਾਰੀ ਰੱਖਣਾ?",
                ["ClearHistoryConfirm"] = "ਇਹ ਸਾਰੇ ਖੋਜ ਇਤਿਹਾਸ ਨੂੰ ਮਿਟਾ ਦੇਵੇਗਾ। ਜਾਰੀ ਰੱਖਣਾ?",
                ["CacheClearedSuccess"] = "ਕੈਸ਼ ਸਫਲਤਾਪੂਰਵਕ ਸਾਫ਼ ਹੋ ਗਿਆ",
                ["HistoryClearedSuccess"] = "ਇਤਿਹਾਸ ਸਫਲਤਾਪੂਰਵਕ ਸਾਫ਼ ਹੋ ਗਿਆ",
                ["Home"] = "ਘਰ",
                ["Welcome"] = "ਸੁਆਗਤ ਹੈ",
                ["CropDiseaseDetection"] = "ਫਸਲ ਬਿਮਾਰੀ ਖੋਜ",
                ["VoiceAssistant"] = "ਆਵਾਜ਼ ਸਹਾਇਕ",
                ["History"] = "ਇਤਿਹਾਸ",
                ["Farmer"] = "ਕਿਸਾਨ",
                ["KrishiAI"] = "ਕ੍ਰਿਸ਼ੀ ਏਆਈ",
                ["YourAIFarmingCompanion"] = "ਤੁਹਾਡਾ ਏਆਈ ਖੇਤੀ ਸਾਥੀ",
                ["SmartFarming"] = "ਸਮਾਰਟ ਖੇਤੀ ਲਈ",
                ["BetterTomorrow"] = "ਬਿਹਤਰ ਕੱਲ੍ਹ",
                ["HowCanIHelp"] = "ਅੱਜ ਮੈਂ ਤੁਹਾਡੀ ਕਿਵੇਂ ਮਦਦ ਕਰ ਸਕਦਾ ਹਾਂ?",
                ["QuickFeatures"] = "ਤੇਜ਼ ਵਿਸ਼ੇਸ਼ਤਾਵਾਂ",
                ["Languages"] = "8 ਭਾਸ਼ਾਵਾਂ",
                ["FarmingTips"] = "ਖੇਤੀ ਸੁਝਾਅ",
                ["DetectDisease"] = "ਬਿਮਾਰੀ ਖੋਜੋ",
                ["AskQuestion"] = "ਸਵਾਲ ਪੁੱਛੋ",
                ["Notifications"] = "ਸੂਚਨਾਵਾਂ",
                ["ClearNotifications"] = "ਸੂਚਨਾਵਾਂ ਸਾਫ਼ ਕਰੋ",
                ["ChooseLanguage"] = "ਭਾਸ਼ਾ ਚੁਣੋ",

                // Descriptions
                ["AppDescription"] = "ਕ੍ਰਿਸ਼ੀ ਏਆਈ ਤੁਹਾਡਾ ਏਆਈ-ਸੰਚਾਲਿਤ ਖੇਤੀ ਸਹਾਇਕ ਹੈ ਜੋ ਫਸਲ ਦੀਆਂ ਬਿਮਾਰੀਆਂ ਦਾ ਪਤਾ ਲਗਾਉਣ ਅਤੇ ਕਈ ਭਾਰਤੀ ਭਾਸ਼ਾਵਾਂ ਵਿੱਚ ਖੇਤੀ ਸਲਾਹ ਪ੍ਰਦਾਨ ਕਰਨ ਵਿੱਚ ਮਦਦ ਕਰਦਾ ਹੈ।",
                ["FeaturesTitle"] = "ਵਿਸ਼ੇਸ਼ਤਾਵਾਂ:",
                ["Feature1Text"] = "• ਆਨ-ਡਿਵਾਈਸ ਏਆਈ ਬਿਮਾਰੀ ਖੋਜ",
                ["Feature2Text"] = "• ਬਹੁਭਾਸ਼ੀ ਆਵਾਜ਼ ਸਹਾਇਕ",
                ["Feature3Text"] = "• ਆਫਲਾਈਨ-ਪਹਿਲਾਂ ਆਰਕੀਟੈਕਚਰ",
                ["Feature4Text"] = "• ਇਲਾਜ ਸਿਫਾਰਸ਼ਾਂ",
                ["CopyrightText"] = "© 2026 ਕ੍ਰਿਸ਼ੀ ਏਆਈ ਟੀਮ",
                ["CropDiseaseDescription"] = "ਆਪਣੀ ਫਸਲ ਦੀ ਫੋਟੋ ਲਓ ਅਤੇ ਇਲਾਜ ਸਿਫਾਰਸ਼ਾਂ ਦੇ ਨਾਲ ਤੁਰੰਤ ਬਿਮਾਰੀ ਦੀ ਪਛਾਣ ਪ੍ਰਾਪਤ ਕਰੋ.",
                ["VoiceAssistantDescription"] = "ਆਪਣੀ ਭਾਸ਼ਾ ਵਿੱਚ ਖੇਤੀ ਦੇ ਸਵਾਲ ਪੁੱਛੋ ਅਤੇ ਤੁਰੰਤ ਏਆਈ-ਸੰਚਾਲਿਤ ਸਲਾਹ ਪ੍ਰਾਪਤ ਕਰੋ",

                // Additional Crop Disease fields
                ["NoImageSelected"] = "ਕੋਈ ਚਿੱਤਰ ਚੁਣਿਆ ਨਹੀਂ",
                ["CaptureOrSelectImage"] = "ਫਸਲ ਦੀ ਤਸਵੀਰ ਕੈਪਚਰ ਕਰੋ ਜਾਂ ਚੁਣੋ",
                ["Capture"] = "ਕੈਪਚਰ",
                ["Gallery"] = "ਗੈਲਰੀ",
                ["AnalyzeDisease"] = "ਬਿਮਾਰੀ ਦਾ ਵਿਸ਼ਲੇਸ਼ਣ ਕਰੋ",
                ["AnalyzingImage"] = "ਫਸਲ ਤਸਵੀਰ ਦਾ ਵਿਸ਼ਲੇਸ਼ਣ ਕਰ ਰਿਹਾ ਹੈ...",
                ["DetectionResults"] = "ਖੋਜ ਨਤੀਜੇ",
                ["Confidence"] = "ਭਰੋਸਾ",
                ["Severity"] = "ਗੰਭੀਰਤਾ",
                ["TreatmentRecommendations"] = "ਇਲਾਜ ਸਿਫਾਰਸ਼ਾਂ",
                ["OrganicTreatment"] = "ਜੈਵਿਕ ਇਲਾਜ",
                ["ChemicalTreatment"] = "ਰਸਾਇਣਿਕ ਇਲਾਜ",
                ["PreventionTips"] = "ਰੋਕਥਾਮ ਸੁਝਾਅ",
                ["NewAnalysis"] = "ਨਵਾਂ ਵਿਸ਼ਲੇਸ਼ਣ",
                ["SelectLanguage"] = "ਭਾਸ਼ਾ ਚੁਣੋ",
                ["Listening"] = "ਸੁਣ ਰਿਹਾ ਹੈ...",
                ["TapToSpeak"] = "ਬੋਲਣ ਲਈ ਟੈਪ ਕਰੋ",
                ["ClearChat"] = "ਚੈਟ ਸਾਫ਼ ਕਰੋ",
                ["DiseaseDetectionHistory"] = "ਬਿਮਾਰੀ ਖੋਜ ਇਤਿਹਾਸ",
                ["NoHistoryYet"] = "ਅਜੇ ਤੱਕ ਕੋਈ ਇਤਿਹਾਸ ਨਹੀਂ",
                ["StartAnalyzingCrops"] = "ਇੱਥੇ ਇਤਿਹਾਸ ਵੇਖਣ ਲਈ ਫਸਲਾਂ ਦਾ ਵਿਸ਼ਲੇਸ਼ਣ ਸ਼ੁਰੂ ਕਰੋ",
                ["ViewDetails"] = "ਵੇਰਵੇ ਵੇਖੋ",
                ["FarmerTipsTitle"] = "ਕਿਸਾਨ ਸੁਝਾਅ",

                // Greetings
                ["GoodMorning"] = "ਸਤ ਸ੍ਰੀ ਅਕਾਲ",
                ["GoodAfternoon"] = "ਸ਼ੁਭ ਦੁਪਹਿਰ",
                ["GoodEvening"] = "ਸ਼ੁਭ ਸ਼ਾਮ",
                ["GoodNight"] = "ਸ਼ੁਭ ਰਾਤ",

                // History Page
                ["Refresh"] = "ਰਿਫ੍ਰੈਸ਼ ਕਰੋ",
                ["ClearAllHistory"] = "ਸਾਰਾ ਇਤਿਹਾਸ ਸਾਫ਼ ਕਰੋ"
            },
            ["gu-IN"] = new()
            {
                ["Settings"] = "સેટિંગ્સ",
                ["Preferences"] = "પસંદગીઓ",
                ["DefaultLanguage"] = "ડિફૉલ્ટ ભાષા",
                ["LanguageHelpText"] = "ભાષા સંપૂર્ણ એપ્લિકેશનમાં લાગુ થશે",
                ["SaveHistory"] = "ઇતિહાસ સાચવો",
                ["SaveHistoryDescription"] = "શોધ પરિણામોને સ્થાનિક રીતે સ્ટોર કરો",
                ["AutoPlayResponses"] = "ઓટો પ્લે પ્રતિસાદ",
                ["AutoPlayResponsesDescription"] = "વૉઇસ પ્રતિસાદ આપમેળે ચલાવો",
                ["SaveSettings"] = "સેટિંગ્સ સાચવો",
                ["DataManagement"] = "ડેટા વ્યવસ્થાપન",
                ["ClearCache"] = "🗑️ કેશ સાફ કરો",
                ["ClearHistory"] = "🗑️ ઇતિહાસ સાફ કરો",
                ["About"] = "વિશે",
                ["Success"] = "સફળતા",
                ["Error"] = "ભૂલ",
                ["Yes"] = "હા",
                ["No"] = "ના",
                ["OK"] = "બરાબર",
                ["Cancel"] = "રદ કરો",
                ["SettingsSavedSuccess"] = "સેટિંગ્સ સફળતાપૂર્વક સાચવી. ભાષા પરિવર્તન સંપૂર્ણ એપ્લિકેશનમાં લાગુ થશે.",
                ["ClearCacheConfirm"] = "આ તમામ કેશ કરેલી છબીઓ કાઢી નાખશે. ચાલુ રાખશો?",
                ["ClearHistoryConfirm"] = "આ તમામ શોધ ઇતિહાસ કાઢી નાખશે. ચાલુ રાખશો?",
                ["CacheClearedSuccess"] = "કેશ સફળતાપૂર્વક સાફ થઈ",
                ["HistoryClearedSuccess"] = "ઇતિહાસ સફળતાપૂર્વક સાફ થયો",
                ["Home"] = "હોમ",
                ["Welcome"] = "સ્વાગત છે",
                ["CropDiseaseDetection"] = "પાક રોગ શોધ",
                ["VoiceAssistant"] = "વૉઇસ સહાયક",
                ["History"] = "ઇતિહાસ",
                ["Farmer"] = "ખેડૂત",
                ["KrishiAI"] = "કૃષિ એઆઈ",
                ["YourAIFarmingCompanion"] = "તમારો એઆઈ ખેતી સાથી",
                ["SmartFarming"] = "સ્માર્ટ ખેતી માટે",
                ["BetterTomorrow"] = "સારી આવતીકાલ",
                ["HowCanIHelp"] = "આજે હું તમને કેવી રીતે મદદ કરી શકું?",
                ["QuickFeatures"] = "ઝડપી સુવિધાઓ",
                ["Languages"] = "8 ભાષાઓ",
                ["FarmingTips"] = "ખેતી ટિપ્સ",
                ["DetectDisease"] = "રોગ શોધો",
                ["AskQuestion"] = "પ્રશ્ન પૂછો",
                ["Notifications"] = "સૂચનાઓ",
                ["ClearNotifications"] = "સૂચનાઓ સાફ કરો",
                ["ChooseLanguage"] = "ભાષા પસંદ કરો",

                // Descriptions
                ["AppDescription"] = "કૃષિ એઆઈ તમારો એઆઈ-સંચાલિત ખેતી સહાયક છે જે પાક રોગોની શોધ કરવામાં અને અનેક ભારતીય ભાષાઓમાં કૃષિ સલાહ આપવામાં મદદ કરે છે।",
                ["FeaturesTitle"] = "સુવિધાઓ:",
                ["Feature1Text"] = "• ઓન-ડિવાઈસ એઆઈ રોગ શોધ",
                ["Feature2Text"] = "• બહુભાષી વૉઇસ સહાયક",
                ["Feature3Text"] = "• ઓફલાઇન-ફર્સ્ટ આર્કિટેક્ચર",
                ["Feature4Text"] = "• સારવાર ભલામણો",
                ["CopyrightText"] = "© 2026 કૃષિ એઆઈ ટીમ",
                ["CropDiseaseDescription"] = "તમારા પાકનો ફોટો લો અને સારવાર ભલામણો સાથે તુરંત રોગની ઓળખ મેળવો.",
                ["VoiceAssistantDescription"] = "તમારી ભાષામાં ખેતી પ્રશ્નો પૂછો અને તુરંત એઆઈ-સંચાલિત સલાહ મેળવો",

                // Additional Crop Disease fields
                ["NoImageSelected"] = "કોઈ છબી પસંદ કરવામાં આવી નથી",
                ["CaptureOrSelectImage"] = "પાકની છબી કેપ્ચર કરો અથવા પસંદ કરો",
                ["Capture"] = "કેપ્ચર",
                ["Gallery"] = "ગેલેરી",
                ["AnalyzeDisease"] = "રોગનું વિશ્લેષણ કરો",
                ["AnalyzingImage"] = "પાકની છબીનું વિશ્લેષણ કરી રહ્યું છે...",
                ["DetectionResults"] = "શોધ પરિણામો",
                ["Confidence"] = "વિશ્વાસ",
                ["Severity"] = "ગંભીરતા",
                ["TreatmentRecommendations"] = "સારવાર ભલામણો",
                ["OrganicTreatment"] = "કાર્બનિક સારવાર",
                ["ChemicalTreatment"] = "રાસાયણિક સારવાર",
                ["PreventionTips"] = "નિવારણ ટિપ્સ",
                ["NewAnalysis"] = "નવું વિશ્લેષણ",
                ["SelectLanguage"] = "ભાષા પસંદ કરો",
                ["Listening"] = "સાંભળી રહ્યું છે...",
                ["TapToSpeak"] = "બોલવા માટે ટેપ કરો",
                ["ClearChat"] = "ચેટ સાફ કરો",
                ["DiseaseDetectionHistory"] = "રોગ શોધ ઇતિહાસ",
                ["NoHistoryYet"] = "હજી સુધી કોઈ ઇતિહાસ નથી",
                ["StartAnalyzingCrops"] = "અહીં ઇતિહાસ જોવા માટે પાકનું વિશ્લેષણ શરૂ કરો",
                ["ViewDetails"] = "વિગતો જુઓ",
                ["FarmerTipsTitle"] = "ખેડૂત ટિપ્સ",

                // Greetings
                ["GoodMorning"] = "સુપ્રભાત",
                ["GoodAfternoon"] = "શુભ બપોર",
                ["GoodEvening"] = "શુભ સાંજ",
                ["GoodNight"] = "શુભ રાત્રિ",

                // History Page
                ["Refresh"] = "રિફ્રેશ કરો",
                ["ClearAllHistory"] = "બધો ઇતિહાસ સાફ કરો"
            },
            ["bn-IN"] = new()
            {
                ["Settings"] = "সেটিংস",
                ["Preferences"] = "পছন্দসমূহ",
                ["DefaultLanguage"] = "ডিফল্ট ভাষা",
                ["LanguageHelpText"] = "ভাষা সম্পূর্ণ অ্যাপ জুড়ে প্রয়োগ করা হবে",
                ["SaveHistory"] = "ইতিহাস সংরক্ষণ করুন",
                ["SaveHistoryDescription"] = "সনাক্তকরণ ফলাফল স্থানীয়ভাবে সংরক্ষণ করুন",
                ["AutoPlayResponses"] = "অটো প্লে প্রতিক্রিয়া",
                ["AutoPlayResponsesDescription"] = "স্বয়ংক্রিয়ভাবে ভয়েস প্রতিক্রিয়া চালান",
                ["SaveSettings"] = "সেটিংস সংরক্ষণ করুন",
                ["DataManagement"] = "ডেটা ব্যবস্থাপনা",
                ["ClearCache"] = "🗑️ ক্যাশে সাফ করুন",
                ["ClearHistory"] = "🗑️ ইতিহাস সাফ করুন",
                ["About"] = "সম্পর্কে",
                ["Success"] = "সফলতা",
                ["Error"] = "ত্রুটি",
                ["Yes"] = "হ্যাঁ",
                ["No"] = "না",
                ["OK"] = "ঠিক আছে",
                ["Cancel"] = "বাতিল করুন",
                ["SettingsSavedSuccess"] = "সেটিংস সফলভাবে সংরক্ষিত হয়েছে। ভাষা পরিবর্তন সম্পূর্ণ অ্যাপ জুড়ে প্রয়োগ করা হবে।",
                ["ClearCacheConfirm"] = "এটি সমস্ত ক্যাশে করা ছবি মুছে ফেলবে। চালিয়ে যেতে চান?",
                ["ClearHistoryConfirm"] = "এটি সমস্ত সনাক্তকরণ ইতিহাস মুছে ফেলবে। চালিয়ে যেতে চান?",
                ["CacheClearedSuccess"] = "ক্যাশে সফলভাবে সাফ করা হয়েছে",
                ["HistoryClearedSuccess"] = "ইতিহাস সফলভাবে সাফ করা হয়েছে",
                ["Home"] = "হোম",
                ["Welcome"] = "স্বাগতম",
                ["CropDiseaseDetection"] = "ফসলের রোগ সনাক্তকরণ",
                ["VoiceAssistant"] = "ভয়েস সহায়ক",
                ["History"] = "ইতিহাস",
                ["Farmer"] = "কৃষক",
                ["KrishiAI"] = "কৃষি এআই",
                ["YourAIFarmingCompanion"] = "আপনার এআই কৃষি সঙ্গী",
                ["SmartFarming"] = "স্মার্ট কৃষির জন্য",
                ["BetterTomorrow"] = "ভালো আগামীকালের জন্য",
                ["HowCanIHelp"] = "আজ আমি আপনাকে কীভাবে সাহায্য করতে পারি?",
                ["QuickFeatures"] = "দ্রুত বৈশিষ্ট্য",
                ["Languages"] = "৮টি ভাষা",
                ["FarmingTips"] = "কৃষি টিপস",
                ["DetectDisease"] = "রোগ সনাক্ত করুন",
                ["AskQuestion"] = "প্রশ্ন করুন",
                ["Notifications"] = "বিজ্ঞপ্তি",
                ["ClearNotifications"] = "বিজ্ঞপ্তি সাফ করুন",
                ["ChooseLanguage"] = "ভাষা নির্বাচন করুন",

                // Descriptions
                ["AppDescription"] = "কৃষি এআই আপনার এআই-চালিত কৃষি সঙ্গী যা ফসলের রোগ সনাক্ত করতে এবং একাধিক ভারতীয় ভাষায় কৃষি পরামর্শ প্রদান করতে সাহায্য করে।",
                ["FeaturesTitle"] = "বৈশিষ্ট্য:",
                ["Feature1Text"] = "• অন-ডিভাইস এআই রোগ সনাক্তকরণ",
                ["Feature2Text"] = "• বহুভাষিক ভয়েস সহায়ক",
                ["Feature3Text"] = "• অফলাইন-ফার্স্ট আর্কিটেক্চার",
                ["Feature4Text"] = "• চিকিৎসা সুপারিশ",
                ["CopyrightText"] = "© 2026 কৃষি এআই টিম",
                ["CropDiseaseDescription"] = "আপনার ফসলের ফটো তুলুন এবং চিকিৎসা সুপারিশ সহ তাৎক্ষণিক রোগ সনাক্তকরণ পান।",
                ["VoiceAssistantDescription"] = "আপনার ভাষায় কৃষি প্রশ্ন করুন এবং তাৎক্ষণিকভাবে এআই-চালিত পরামর্শ পান",

                // Additional Crop Disease fields
                ["NoImageSelected"] = "কোনো ছবি নির্বাচন করা হয়নি",
                ["CaptureOrSelectImage"] = "ফসলের ছবি ক্যাপচার বা নির্বাচন করুন",
                ["Capture"] = "ক্যাপচার",
                ["Gallery"] = "গ্যালারি",
                ["AnalyzeDisease"] = "রোগ বিশ্লেষণ করুন",
                ["AnalyzingImage"] = "ফসলের ছবি বিশ্লেষণ করা হচ্ছে...",
                ["DetectionResults"] = "সনাক্তকরণ ফলাফল",
                ["Confidence"] = "আত্মবিশ্বাস",
                ["Severity"] = "তীব্রতা",
                ["TreatmentRecommendations"] = "চিকিৎসা সুপারিশ",
                ["OrganicTreatment"] = "জৈব চিকিৎসা",
                ["ChemicalTreatment"] = "রাসায়নিক চিকিৎসা",
                ["PreventionTips"] = "প্রতিরোধ টিপস",
                ["NewAnalysis"] = "নতুন বিশ্লেষণ",
                ["SelectLanguage"] = "ভাষা নির্বাচন করুন",
                ["Listening"] = "শুনছে...",
                ["TapToSpeak"] = "কথা বলতে ট্যাপ করুন",
                ["ClearChat"] = "চ্যাট সাফ করুন",
                ["DiseaseDetectionHistory"] = "রোগ সনাক্তকরণ ইতিহাস",
                ["NoHistoryYet"] = "এখনও কোনো ইতিহাস নেই",
                ["StartAnalyzingCrops"] = "এখানে ইতিহাস দেখতে ফসল বিশ্লেষণ শুরু করুন",
                ["ViewDetails"] = "বিস্তারিত দেখুন",
                ["FarmerTipsTitle"] = "কৃষক টিপস",

                // Greetings
                ["GoodMorning"] = "সুপ্রভাত",
                ["GoodAfternoon"] = "শুভ অপরাহ্ন",
                ["GoodEvening"] = "শুভ সন্ধ্যা",
                ["GoodNight"] = "শুভ রাত্রি",

                // History Page
                ["Refresh"] = "রিফ্রেশ করুন",
                ["ClearAllHistory"] = "সমস্ত ইতিহাস সাফ করুন"
            }
        };

        // Try to get translation for current language
        if (translations.ContainsKey(currentLanguage) && translations[currentLanguage].ContainsKey(key))
        {
            return translations[currentLanguage][key];
        }

        // Fallback to English
        if (translations["en-US"].ContainsKey(key))
        {
            return translations["en-US"][key];
        }

        // Last resort - return key itself
        return key;
    }
}
