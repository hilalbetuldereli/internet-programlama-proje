# 🍳 TarifDefteri

ASP.NET Core MVC kullanılarak geliştirilen yemek tarifi paylaşım platformu.

## 📌 Proje Hakkında

**TarifDefteri**, kullanıcıların yemek tariflerini paylaşabildiği, yorum yapabildiği ve favori tariflerini takip edebildiği web tabanlı bir uygulamadır.
Proje, **ASP.NET Core 9.0 MVC** mimarisi ve **Entity Framework Core** kullanılarak geliştirilmiştir.

---

## ✨ Temel Özellikler

* Kullanıcı kayıt ve giriş sistemi
* Rol bazlı yapı (**Admin, Chef, User**)
* Tarif ekleme, düzenleme ve silme
* Yorum yapma ve 1–5 yıldız puanlama
* Favorilere ekleme
* Admin onay sistemi
* Responsive arayüz (Bootstrap 5)

---

## 🛠️ Kullanılan Teknolojiler

* **ASP.NET Core 9.0 MVC**
* **C#**
* **Entity Framework Core**
* **SQLite**
* **Bootstrap 5**
* **Razor View Engine**

---

## 🚀 Kurulum

1. Repoyu klonlayın:

```bash
git clone https://github.com/hilalbetuldereli/internet-programlama-proje.git
cd TarifDefteri
```

2. Projeyi çalıştırın:

```bash
dotnet restore
dotnet run
```

3. Tarayıcıdan açın:

```
http://localhost:5070
```

> Veritabanı otomatik olarak oluşturulur.

---

## 👥 Kullanıcı Rolleri

| Rol       | Yetkiler                                       |
| --------- | ---------------------------------------------- |
| **Admin** | Kullanıcı onaylama, kategori ve tarif yönetimi |
| **Chef**  | Kendi tariflerini ekleme ve düzenleme          |
| **User**  | Tarif görüntüleme, yorum yapma, puanlama       |

* Chef ve Admin hesapları **admin onayı** gerektirir.
* Normal kullanıcılar doğrudan giriş yapabilir.

---

## 🗄️ Veritabanı Yapısı (Özet)

* **Users**
* **Categories**
* **Recipes**
* **Comments**
* **Ratings**
* **Favorites**

Tablolar arasında 1-N ilişkiler kurulmuştur ve **Code-First** yaklaşımı kullanılmıştır.

---

## 🔐 Güvenlik

* Session tabanlı kimlik doğrulama
* Rol bazlı yetkilendirme
* Entity Framework sayesinde SQL Injection koruması

> Not: Production ortamı için şifrelerin hash’lenmesi önerilir.

---

## 📂 Proje Yapısı

```
TarifDefteri/
│
├── Controllers/              # MVC Controller sınıfları
├── Data/                     # DbContext ve veritabanı yapılandırmaları
├── Models/                   # Entity ve ViewModel sınıfları
├── Properties/               # Proje yapılandırma dosyaları
├── Views/                    # Razor View dosyaları
├── wwwroot/                  # CSS, JS ve statik dosyalar
│
├── appsettings.json
├── appsettings.Development.json
├── Program.cs                # Uygulama başlangıç noktası
├── TarifDefteri.csproj       # Proje dosyası
├── README.md                 # Proje dokümantasyonu
└── .gitignore

```

---

