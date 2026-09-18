(function (window) {
  "use strict";

  window.AppConfig = {
    branding: {
      applicationName: "Yazılım Envanteri ve Yönetim Portalı",
      subtitle: "Personel • Uygulamalar • Altyapı • Daha Bağlantılı Bir Organizasyon",
      masthead: {
        rightTop: ["Tek Platform", "Tam Görünürlük", "Daha Güçlü Yönetim"],
        rightBottomLine: "Takip Et • Yönet • İş Birliği Yap",
        rightBottomAccent: "Daha Akıllı Sistemler Oluştur"
      },
      tagline: ["Daha İyi Uygulamalar", "Daha Güçlü", "Kamu Hizmetleri"]
    },

    navigation: [
      { key: "home", label: "Ana Sayfa", icon: "home", href: "/", enabled: true },
      { key: "projects", label: "Projelerim", icon: "projects", href: "/Proje", enabled: true },
      { key: "infrastructure", label: "Altyapı", icon: "server", href: "#", enabled: false },
      { key: "reports", label: "Raporlar", icon: "chart", href: "#", enabled: false },
      { key: "personnel", label: "Personel", icon: "users", href: "#", enabled: false },
      { key: "settings", label: "Ayarlar", icon: "settings", href: "#", enabled: false }
    ],

    currentUser: {
      adSoyad: "Kullanıcı",
      rol: "Görüntüleyici",
      basHarfler: "KU"
    },

    labels: {
      project: {
        code: "Proje Kodu",
        name: "Proje Adı",
        serviceDomain: "Hizmet Alanı",
        description: "Açıklama",
        status: "Durum",
        criticality: "Kritiklik",
        active: "Aktiflik Durumu",
        unit: "Birim",
        parentUnit: "Üst Birim",
        childUnit: "Alt Birim",
        server: "Sunucu",
        websiteUrl: "Web Adresi",
        specialist: "Yazılım Uzmanı"
      },
      specialist: {
        fullName: "Ad Soyad",
        username: "Kullanıcı Adı",
        role: "Görev",
        email: "E-posta",
        phone: "Telefon",
        vendor: "Sorumlu Firma",
        unit: "Birim"
      },
      technology: {
        backend: "Backend Teknolojisi",
        frontend: "Frontend Teknolojisi",
        database: "Veritabanı",
        integration: "Entegrasyon Durumu",
        eSignature: "E-İmza Desteği"
      }
    },

    statusThemes: {
      "Analiz": { label: "Analiz", tone: "purple" },
      "Geliştirme": { label: "Geliştirme", tone: "info" },
      "İnceleme": { label: "İnceleme", tone: "warning" },
      "Test": { label: "Test", tone: "orange" },
      "Tamamlanmış": { label: "Tamamlanmış", tone: "teal" },
      "Yayında": { label: "Yayında", tone: "success" }
    },

    criticalityThemes: {
      "Düşük": { label: "Düşük", tone: "success" },
      "Orta": { label: "Orta", tone: "warning" },
      "Yüksek": { label: "Yüksek", tone: "danger" }
    },

    projectColumns: [
      { key: "projeKodu", label: "Proje Kodu", sortable: true },
      { key: "projeAdi", label: "Proje Adı", sortable: true },
      { key: "projeHizmetAlani", label: "Hizmet Alanı", sortable: false },
      { key: "birim", label: "Birim", sortable: false },
      { key: "yazilimUzmaniAdSoyad", label: "Yazılım Uzmanı", sortable: false },
      { key: "sunucu", label: "Sunucu / Web Adresi", sortable: false },
      { key: "projeDurum", label: "Durum", type: "status", sortable: true },
      { key: "projeKritiklik", label: "Kritiklik", type: "criticality", sortable: true }
    ],

    detailSections: [
      { id: "general", title: "Genel ve Organizasyon Bilgileri", theme: "blue", icon: "project" },
      { id: "infrastructure", title: "Sunucu ve Yayın Bilgileri", theme: "teal", icon: "server" },
      { id: "technology", title: "Teknoloji Yığını ve Entegrasyonlar", theme: "purple", icon: "layers" },
      { id: "developer", title: "Sorumlu Yazılım Uzmanı", theme: "orange", icon: "user" }
    ],

    pageSizeOptions: [5, 10, 20, 50],
    defaultPageSize: 10,

    icons: {
      home: '<path d="M3 11l9-8 9 8" /><path d="M5 10v10h14V10" />',
      projects: '<rect x="3" y="3" width="7" height="7" rx="1.5" /><rect x="14" y="3" width="7" height="7" rx="1.5" /><rect x="3" y="14" width="7" height="7" rx="1.5" /><rect x="14" y="14" width="7" height="7" rx="1.5" />',
      server: '<rect x="3" y="4" width="18" height="6" rx="1.5" /><rect x="3" y="14" width="18" height="6" rx="1.5" /><circle cx="7" cy="7" r="0.5" fill="currentColor" /><circle cx="7" cy="17" r="0.5" fill="currentColor" />',
      chart: '<path d="M4 20V10" /><path d="M12 20V4" /><path d="M20 20v-7" />',
      users: '<circle cx="9" cy="8" r="3.2" /><path d="M2.5 20c1-3.5 3.6-5.5 6.5-5.5s5.5 2 6.5 5.5" /><circle cx="17.5" cy="9" r="2.3" /><path d="M15.8 14.2c2.4.4 4.2 2 5 5.3" />',
      settings: '<circle cx="12" cy="12" r="3" /><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.9l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-1.9-.3 1.7 1.7 0 0 0-1 1.6V21a2 2 0 1 1-4 0v-.2a1.7 1.7 0 0 0-1-1.6 1.7 1.7 0 0 0-1.9.3l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0 .3-1.9 1.7 1.7 0 0 0-1.6-1H3a2 2 0 1 1 0-4h.2a1.7 1.7 0 0 0 1.6-1 1.7 1.7 0 0 0-.3-1.9l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.9.3H9a1.7 1.7 0 0 0 1-1.6V3a2 2 0 1 1 4 0v.2a1.7 1.7 0 0 0 1 1.6 1.7 1.7 0 0 0 1.9-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.9V9a1.7 1.7 0 0 0 1.6 1H21a2 2 0 1 1 0 4h-.2a1.7 1.7 0 0 0-1.6 1z" />',
      menu: '<path d="M4 6h16" /><path d="M4 12h16" /><path d="M4 18h16" />',
      close: '<path d="M6 6l12 12" /><path d="M18 6L6 18" />',
      bell: '<path d="M6 9a6 6 0 1 1 12 0c0 3 1 4.5 1.5 5.5H4.5C5 13.5 6 12 6 9Z" /><path d="M9.5 17a2.5 2.5 0 0 0 5 0" />',
      search: '<circle cx="11" cy="11" r="7" /><path d="M21 21l-4.3-4.3" />',
      chevronDown: '<path d="M6 9l6 6 6-6" />',
      arrowLeft: '<path d="M19 12H5" /><path d="M11 18l-6-6 6-6" />',
      externalLink: '<path d="M14 4h6v6" /><path d="M20 4L10 14" /><path d="M18 14v6H4V6h6" />',
      copy: '<rect x="9" y="9" width="11" height="11" rx="1.5" /><path d="M5 15H4a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1h10a1 1 0 0 1 1 1v1" />',
      moreHorizontal: '<circle cx="5" cy="12" r="1.4" /><circle cx="12" cy="12" r="1.4" /><circle cx="19" cy="12" r="1.4" />',
      project: '<rect x="3" y="3" width="7" height="7" rx="1.5" /><rect x="14" y="3" width="7" height="7" rx="1.5" /><rect x="3" y="14" width="7" height="7" rx="1.5" /><rect x="14" y="14" width="7" height="7" rx="1.5" />',
      layers: '<path d="M12 3l9 5-9 5-9-5 9-5Z" /><path d="M3 13l9 5 9-5" /><path d="M3 17l9 5 9-5" />',
      user: '<circle cx="12" cy="8" r="4" /><path d="M4 21c1.2-4.2 4.3-6.5 8-6.5s6.8 2.3 8 6.5" />',
      mail: '<rect x="3" y="5" width="18" height="14" rx="2" /><path d="M4 7l8 6 8-6" />',
      phone: '<path d="M6.5 4h3l1.5 4.5-2 1.5a12 12 0 0 0 5 5l1.5-2 4.5 1.5v3a2 2 0 0 1-2 2C10.5 19.5 4.5 13.5 4.5 6a2 2 0 0 1 2-2Z" />',
      inbox: '<path d="M3 12l3-8h12l3 8" /><path d="M3 12v6a1 1 0 0 0 1 1h16a1 1 0 0 0 1-1v-6" /><path d="M3 12h5.5a1 1 0 0 1 1 .7l.5 1.6a1 1 0 0 0 1 .7h2a1 1 0 0 0 1-.7l.5-1.6a1 1 0 0 1 1-.7H21" />',
      alertTriangle: '<path d="M12 4l9 16H3L12 4Z" /><path d="M12 10v4" /><path d="M12 17h.01" />',
      sortAsc: '<path d="M7 15l5-5 5 5" />',
      sortDesc: '<path d="M7 9l5 5 5-5" />',
      sortNeutral: '<path d="M8 9l4-4 4 4" /><path d="M16 15l-4 4-4-4" />'
    }
  };

  window.AppConfig.icon = function (key, size) {
    var s = size || 18;
    var inner = window.AppConfig.icons[key] || "";
    return '<svg width="' + s + '" height="' + s + '" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' + inner + "</svg>";
  };
})(window);
