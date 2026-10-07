namespace Bookify.Web.Seeds
{
    public static class DefaultGovernorates
    {
        public static async Task SeedGovernorates(ApplicationDbContext context)
        {
            if (await context.Governorates.AnyAsync())
                return;

            var governorates = new Dictionary<string, string[]>
            {
                ["Cairo"] = new[] { "Nasr City", "Heliopolis", "Maadi", "Zamalek", "Downtown", "Shubra", "New Cairo", "Helwan", "El Marg", "Ain Shams" },
                ["Giza"] = new[] { "Dokki", "Mohandessin", "Haram", "Faisal", "Imbaba", "6th of October", "Sheikh Zayed", "Agouza" },
                ["Alexandria"] = new[] { "Smouha", "Sidi Gaber", "Miami", "Montaza", "Agami", "Borg El Arab", "Stanley", "Mandara" },
                ["Qalyubia"] = new[] { "Banha", "Shubra El Kheima", "Qalyub", "Khanka", "Obour", "Tukh" },
                ["Dakahlia"] = new[] { "Mansoura", "Talkha", "Mit Ghamr", "Dekernes", "Aga", "Sherbin" },
                ["Sharqia"] = new[] { "Zagazig", "10th of Ramadan", "Belbeis", "Minya El Qamh", "Abu Hammad", "Faqous" },
                ["Gharbia"] = new[] { "Tanta", "El Mahalla El Kubra", "Kafr El Zayat", "Zefta", "Samannoud" },
                ["Monufia"] = new[] { "Shibin El Kom", "Menouf", "Sadat City", "Ashmoun", "Quesna" },
                ["Beheira"] = new[] { "Damanhour", "Kafr El Dawwar", "Rashid", "Edku", "Abu Hummus" },
                ["Kafr El Sheikh"] = new[] { "Kafr El Sheikh", "Desouk", "Baltim", "Sidi Salem", "Fuwa" },
                ["Damietta"] = new[] { "Damietta", "New Damietta", "Ras El Bar", "Faraskur", "Kafr Saad" },
                ["Port Said"] = new[] { "Port Fouad", "El Arab", "El Manakh", "El Zohour", "El Dawahy" },
                ["Ismailia"] = new[] { "Ismailia", "Fayed", "Qantara", "Abu Suwir", "Tell El Kebir" },
                ["Suez"] = new[] { "Suez", "Arbaeen", "Ataka", "Faisal", "Ganayen" },
                ["Faiyum"] = new[] { "Faiyum", "Sinnuris", "Tamiya", "Itsa", "Ibsheway" },
                ["Beni Suef"] = new[] { "Beni Suef", "El Wasta", "Nasser", "Biba", "El Fashn" },
                ["Minya"] = new[] { "Minya", "Mallawi", "Samalut", "Maghagha", "Beni Mazar" },
                ["Asyut"] = new[] { "Asyut", "Dairut", "Manfalut", "Abnub", "El Qusiya" },
                ["Sohag"] = new[] { "Sohag", "Akhmim", "Girga", "Tahta", "El Balyana" },
                ["Qena"] = new[] { "Qena", "Nag Hammadi", "Qus", "Dishna", "Farshut" },
                ["Luxor"] = new[] { "Luxor", "Esna", "Armant", "El Tod", "El Qurna" },
                ["Aswan"] = new[] { "Aswan", "Kom Ombo", "Edfu", "Daraw", "Abu Simbel" },
                ["Red Sea"] = new[] { "Hurghada", "Safaga", "El Quseir", "Marsa Alam", "Ras Gharib" },
                ["New Valley"] = new[] { "Kharga", "Dakhla", "Farafra", "Baris", "Balat" },
                ["Matrouh"] = new[] { "Marsa Matrouh", "El Alamein", "El Dabaa", "Siwa", "Sidi Barrani" },
                ["North Sinai"] = new[] { "Arish", "Sheikh Zuweid", "Rafah", "Bir El Abd", "Nakhl" },
                ["South Sinai"] = new[] { "Sharm El Sheikh", "Dahab", "Nuweiba", "El Tor", "Saint Catherine" }
            };

            foreach (var governorate in governorates)
            {
                context.Governorates.Add(new Governorate
                {
                    Name = governorate.Key,
                    Areas = governorate.Value.Select(area => new Area { Name = area }).ToList()
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
