# Penguin Ice Cave Runner - Kurulum Rehberi

Bu doküman, E-health oyun projenize **Ice Cave Runner** seviyesini (Seviye 5) nasıl ekleyeceğinizi açıklar.

## Genel Bakış

Ice Cave Runner, Google Dino tarzında bir sonsuz koşu oyunudur:
- Buz mağarasında koşan penguen karakteri
- Sarkıtlar (tavandan sarkan buzlar) ve dikitler (yerden yükselen buzlar) engeller olarak
- Zıplama (W) ve Eğilme (S) kontrolleri
- Kademeli zorluk sistemi
- Yüksek skor takibi

## Oluşturulan Script'ler

`Assets/Scripts/` klasörüne eklenen script'ler:

1. **PenguinController.cs** - Penguen hareket kontrolü (zıplama/eğilme)
2. **ObstacleMovement.cs** - Engelleri sağdan sola hareket ettirir
3. **ObstacleSpawner.cs** - Artan zorlukla engeller oluşturur
4. **PenguinGameManager.cs** - Oyun durumu, skor ve UI yönetimi

## Unity'de Adım Adım Kurulum

### Adım 1: Sahneyi Oluştur

1. Unity'de yeni bir 2D sahne oluştur: `File > New Scene > 2D (Basic Built-in)`
2. `File > Save As` ile sahneyi `Assets/Scenes/Level4_PenguinRunner.unity` olarak kaydet

### Adım 2: Kamerayı Ayarla

1. **Hierarchy** panelinde **Main Camera**'yı seç
2. **Inspector** panelinde ayarları yap:
   - **Background**: Koyu mavi/turkuaz renk (buz mağarası atmosferi için)
     - Tıkla ve renk seç: R=50, G=100, B=150
   - **Size**: 5-8 arası (tercihen 6)
   - **Position**: (0, 0, -10)

### Adım 3: Layer ve Tag Oluştur

1. `Edit > Project Settings > Tags and Layers` menüsüne git
2. **Layers** kısmında boş bir satıra "Ground" ekle (örn. User Layer 6)
3. **Tags** kısmında `+` butonuna basıp "Obstacle" tag'i ekle

### Adım 4: Zemini Oluştur (Ground)

**Görünür bir zemin oluşturmak için:**

1. **GameObject > 2D Object > Sprites > Square** ile bir kare sprite oluştur
2. **Inspector** panelinde:
   - **İsim**: "Ground" olarak değiştir
   - **Transform**:
     - Position: X=0, Y=-4, Z=0
     - Scale: X=50, Y=1, Z=1
   - **Sprite Renderer**:
     - Color: Açık mavi/beyaz (R=200, G=230, B=255)
3. **Add Component** butonuna tıkla
4. **Box Collider 2D** ekle
   - Otomatik olarak sprite boyutuna ayarlanacak
5. **Layer** dropdown'ından "Ground" seç (üstte, Tag'in yanında)

### Adım 5: Tavanı Oluştur (Opsiyonel - Görsel)

1. **GameObject > 2D Object > Sprites > Square** ile bir kare daha oluştur
2. **İsim**: "Ceiling"
3. **Transform**:
   - Position: X=0, Y=5, Z=0
   - Scale: X=50, Y=1, Z=1
4. **Sprite Renderer**:
   - Color: Koyu gri/mavi (R=80, G=100, B=120)
5. **Collider eklemeyin** - sadece görsel

### Adım 6: Penguen Karakterini Oluştur

1. **GameObject > 2D Object > Sprites > Circle** ile daire oluştur
2. **İsim**: "Penguin" olarak değiştir
3. **Transform**:
   - Position: X=-4, Y=-2, Z=0
   - Scale: X=1.5, Y=2, Z=1 (ovalimsi penguen şekli)
4. **Sprite Renderer**:
   - Color: Beyaz (geçici, sonra penguen sprite'ı eklenecek)

**Bileşenler ekle:**

5. **Add Component > Physics 2D > Rigidbody 2D**
   - Gravity Scale: 0 (script tarafından kontrol edilecek)
   - Freeze Rotation Z: ✓ (işaretle)

6. **Add Component > Physics 2D > Box Collider 2D**
   - Size: X=1, Y=2
   - Offset: X=0, Y=0

7. **Add Component > Scripts > Penguin Controller**
   - **Jump Force**: 12
   - **Gravity**: -25
   - **Ground Layer**: "Ground" seç (dropdown'dan)
   - **Normal Collider Size**: X=1, Y=2
   - **Normal Collider Offset**: X=0, Y=0
   - **Duck Collider Size**: X=1, Y=1
   - **Duck Collider Offset**: X=0, Y=-0.5
   - **Ground Check Radius**: 0.2

### Adım 7: Engel Prefab'larını Oluştur

**Önce Prefabs klasörü oluştur:**
- **Project** panelinde `Assets` klasörüne sağ tıkla
- `Create > Folder` seç, adını "Prefabs" koy
- Prefabs içinde bir klasör daha oluştur: "Obstacles"

#### A) Stalagmit (Zemin Engeli - Dikit)

**Küçük Stalagmit:**

1. **GameObject > 2D Object > Sprites > Triangle** oluştur
2. **İsim**: "Stalagmite_Small"
3. **Transform**:
   - Position: X=0, Y=-3.5, Z=0 (zemin üzerinde)
   - Rotation: Z=0 (yukarı bakan üçgen)
   - Scale: X=0.8, Y=2, Z=1
4. **Sprite Renderer**:
   - Color: Açık mavi (R=180, G=220, B=255)
5. **Add Component > Physics 2D > Box Collider 2D**
   - **Is Trigger**: ✓ İşaretle
   - Size otomatik ayarlanacak
6. **Add Component > Scripts > Obstacle Movement**
7. **Tag**: "Obstacle" seç (üstte)
8. **Project** panelinde `Assets/Prefabs/Obstacles/` klasörünü aç
9. **Hierarchy**'den "Stalagmite_Small"'ı **Project** paneline sürükle (prefab oluştur)
10. Hierarchy'deki orijinali sil

**Orta ve Büyük Varyasyonlar:**
- Aynı işlemi tekrarla, sadece Scale Y'yi değiştir:
  - **Stalagmite_Medium**: Scale Y=2.5
  - **Stalagmite_Large**: Scale Y=3

#### B) Sarkıt (Tavan Engeli - Stalaktit)

**Kısa Stalaktit:**

1. **GameObject > 2D Object > Sprites > Triangle** oluştur
2. **İsim**: "Stalactite_Short"
3. **Transform**:
   - Position: X=0, Y=4, Z=0 (tavandan sarkar)
   - Rotation: Z=180 (aşağı bakan üçgen)
   - Scale: X=0.8, Y=2, Z=1
4. **Sprite Renderer**:
   - Color: Beyaz/açık cyan (R=220, G=240, B=255)
5. **Add Component > Physics 2D > Box Collider 2D**
   - **Is Trigger**: ✓ İşaretle
6. **Add Component > Scripts > Obstacle Movement**
7. **Tag**: "Obstacle"
8. `Assets/Prefabs/Obstacles/` klasörüne sürükle
9. Hierarchy'deki orijinali sil

**Uzun Varyasyonlar:**
- Aynı işlemi tekrarla:
  - **Stalactite_Long**: Scale Y=3
  - **Stalactite_VeryLong**: Scale Y=4

### Adım 8: Engel Oluşturucuyu Ayarla (Obstacle Spawner)

1. **GameObject > Create Empty** ile boş bir obje oluştur
2. **İsim**: "ObstacleSpawner"
3. **Transform**:
   - Position: X=0, Y=0, Z=0
4. **Add Component > Scripts > Obstacle Spawner**

**Spawner ayarlarını yap:**

5. **Obstacle Spawner** bileşeninde:
   - **Stalagmite Prefabs**: Size = 3 yap
     - **Project** panelinden 3 stalagmit prefab'ını sürükle
     - Element 0: Stalagmite_Small
     - Element 1: Stalagmite_Medium
     - Element 2: Stalagmite_Large

   - **Stalactite Prefabs**: Size = 3 yap
     - 3 stalaktit prefab'ını sürükle
     - Element 0: Stalactite_Short
     - Element 1: Stalactite_Long
     - Element 2: Stalactite_VeryLong

   - **Spawn X Position**: 12
   - **Ground Y Position**: -3.5
   - **Ceiling Y Position**: 4
   - **Base Spawn Interval**: 2
   - **Min Spawn Interval**: 0.8
   - **Base Speed**: 5
   - **Max Speed**: 13
   - **Speed Increase Rate**: 0.2
   - **Difficulty Factor**: 0.99

### Adım 9: Oyun Yöneticisini Oluştur (Game Manager)

1. **GameObject > Create Empty**
2. **İsim**: "GameManager"
3. **Add Component > Scripts > Penguin Game Manager**
4. **Score Rate**: 10 (saniyede 10 puan)

### Adım 10: UI (Kullanıcı Arayüzü) Oluştur

#### Canvas Ayarla

1. **GameObject > UI > Canvas** ile bir Canvas oluştur
2. **Canvas** seçiliyken **Inspector**'da:
   - **Canvas Scaler** bileşenini bul
   - **UI Scale Mode**: "Scale With Screen Size"
   - **Reference Resolution**: X=1920, Y=1080

#### Skor Göstergesi (Sol Üst)

1. **Canvas**'a sağ tıkla > **UI > Text - TextMeshPro**
2. İlk kez kullanıyorsan "Import TMP Essentials" butonuna bas
3. **İsim**: "ScoreText"
4. **Rect Transform**:
   - Anchors: Sol üst köşe (preset'ten seç)
   - Pos X: 150, Pos Y: -50
   - Width: 300, Height: 60
5. **Text**: "Score: 0"
6. **Font Size**: 36
7. **Color**: Beyaz

#### Yüksek Skor (Sağ Üst)

1. **Canvas** > sağ tıkla > **UI > Text - TextMeshPro**
2. **İsim**: "HighScoreText"
3. **Rect Transform**:
   - Anchors: Sağ üst köşe
   - Pos X: -150, Pos Y: -50
   - Width: 300, Height: 60
4. **Text**: "High Score: 0"
5. **Font Size**: 30
6. **Color**: Sarı/Altın
7. **Alignment**: Sağa hizala (right align)

#### Game Over Paneli

1. **Canvas** > sağ tıkla > **UI > Panel**
2. **İsim**: "GameOverPanel"
3. **Rect Transform**: Stretch (tüm ekranı kaplasın)
4. **Image** bileşeninde:
   - Color: Siyah, Alpha: 200 (yarı saydam)

**GameOverPanel içine elementler ekle:**

**Oyun Bitti Yazısı:**
1. **GameOverPanel** > sağ tıkla > **UI > Text - TextMeshPro**
2. **İsim**: "GameOverTitle"
3. **Rect Transform**:
   - Anchors: Center-top
   - Pos X: 0, Pos Y: -150
   - Width: 800, Height: 100
4. **Text**: "OYUN BİTTİ"
5. **Font Size**: 72
6. **Color**: Kırmızı
7. **Alignment**: Center

**Final Skor:**
1. **GameOverPanel** > sağ tıkla > **UI > Text - TextMeshPro**
2. **İsim**: "FinalScoreText"
3. **Rect Transform**:
   - Anchors: Center
   - Pos X: 0, Pos Y: 50
   - Width: 500, Height: 80
4. **Text**: "Score: 0"
5. **Font Size**: 48
6. **Alignment**: Center

**Yüksek Skor:**
1. **GameOverPanel** > **UI > Text - TextMeshPro**
2. **İsim**: "GameOverHighScoreText"
3. **Rect Transform**:
   - Pos X: 0, Pos Y: -20
   - Width: 500, Height: 60
4. **Text**: "High Score: 0"
5. **Font Size**: 36
6. **Color**: Altın

**Restart Butonu:**
1. **GameOverPanel** > **UI > Button - TextMeshPro**
2. **İsim**: "RestartButton"
3. **Rect Transform**:
   - Pos X: 0, Pos Y: -120
   - Width: 300, Height: 80
4. Buton içindeki **Text**: "Yeniden Başla"
5. Font Size: 32

**Ana Menü Butonu:**
1. **GameOverPanel** > **UI > Button - TextMeshPro**
2. **İsim**: "MenuButton"
3. **Rect Transform**:
   - Pos X: 0, Pos Y: -220
   - Width: 300, Height: 80
4. Buton içindeki **Text**: "Ana Menü"
5. Font Size: 28

**GameOverPanel'i gizle:**
- **GameOverPanel** seçili iken, **Inspector**'ın en üstünde, ismin yanındaki kutucuğu KAPAT (deaktif et)
- Oyun başlarken panel görünmeyecek, sadece oyun bitince açılacak

### Adım 11: UI'ı Game Manager'a Bağla

1. **Hierarchy**'de **GameManager**'ı seç
2. **Inspector**'da **Penguin Game Manager** bileşeninde:
   - **Score Text**: "ScoreText"'i sürükle
   - **High Score Text**: "HighScoreText"'i sürükle
   - **Game Over Panel**: "GameOverPanel"'i sürükle
   - **Final Score Text**: "FinalScoreText"'i sürükle
   - **Game Over High Score Text**: "GameOverHighScoreText"'i sürükle
   - **Restart Button**: "RestartButton"'u sürükle
   - **Menu Button**: "MenuButton"'u sürükle

### Adım 12: Sahneyi Build Ayarlarına Ekle

1. `File > Build Settings` menüsünü aç
2. **Add Open Scenes** butonuna bas (Level4_PenguinRunner eklenir)
3. "MenuScene"'in de listede olduğundan emin ol
4. Pencereyi kapat

### Adım 13: İlk Testi Yap

1. **File > Save** ile sahneyi kaydet
2. Unity'de **Play** butonuna bas
3. Penguen yere düşmeli ve zemin üzerinde durmalı
4. **W** tuşu ile zıpla - penguen havaya sıçramalı
5. **S** tuşu ile eğil - penguen küçülmeli
6. Birkaç saniye sonra engeller sağdan gelmeye başlamalı
7. Engellere çarpınca "OYUN BİTTİ" paneli görünmeli

**Sorun mu var?** Aşağıdaki "Sorun Giderme" bölümüne bak.

## Menüye Entegrasyon

Ana menüye 5. seviye olarak eklendi:
- LevelManager artık 5 seviyeyi destekliyor
- MenuManager dizileri 5'e genişletildi
- Yeni seviye menüde "Ice Cave Runner" olarak görünecek

**Menüde görünmesi için:**
1. MenuScene sahnesini aç
2. Canvas içindeki Level Button'lara 5. bir buton ekle
3. MenuManager'daki dizilere bağla

## Pixel Art Sprite Oluşturma (Opsiyonel)

Google Dino tarzı otantik pixel art için:

### Penguen Sprite'ı (16x32 piksel)
- Koşma pozunda basit penguen çiz
- Koşma için 2-3 frame
- Zıplama için 1 frame (kanatlar açık)
- Eğilme için 1 frame (çömelmiş)

### Engeller (16x16 - 32x64 piksel)
- Stalagmitler: Yukarı bakan buz sivri uçları
- Stalaktitler: Aşağı sarkan buz damlaları
- Keskin açılı şekiller kullan
- Renkler: Beyaz, açık mavi, cyan

### Unity'de Pixel Art İçe Aktarma:
1. Sprite'ı Unity'de seç
2. Inspector:
   - Texture Type: Sprite (2D and UI)
   - Pixels Per Unit: 16 veya 32
   - Filter Mode: Point (no filter)
   - Compression: None
   - Max Size: 256

### Animasyon Ekleme

1. `Assets/Animators/PenguinAnimator` controller oluştur
2. Animasyonlar:
   - Idle/Run (döngü)
   - Jump (bir kez)
   - Duck (eğilirken döngü)
   - Die (bir kez)
3. Parametreler:
   - Bool: IsGrounded
   - Bool: IsDucking
   - Trigger: Jump
   - Trigger: Die
4. Durumlar arası geçişler oluştur
5. Animator'ı Penguin objesine ekle

## Sorun Giderme

### Penguen zeminden düşüyor
- Ground layer'ının zemin collider'ına atandığını kontrol et
- PenguinController'da doğru Ground Layer seçildiğini kontrol et
- Rigidbody2D collision detection "Continuous" olmalı

### Engeller oluşmuyor
- ObstacleSpawner'da prefab'ların atandığını kontrol et
- Tüm prefab'larda ObstacleMovement script'i olmalı
- GameManager'ın spawner.StartSpawning() çağırdığını kontrol et

### Çarpışmalar çalışmıyor
- Tüm engellerin "Obstacle" tag'i olmalı
- Penguen collider'ı "Is Trigger" değil
- Engel collider'ları "Is Trigger" olmalı

### Oyun çarpışmada bitmiyor
- PenguinGameManager sahnede olmalı
- PenguinController game manager'ı bulmalı
- Engel tag'lerini kontrol et

### Penguen zıplamıyor
- Ground Check pozisyonu doğru mu kontrol et
- Penguen Ground layer üzerinde mi?
- Console'da hata var mı?

## Performans Optimizasyonu

1. Engeller için object pooling kullan (ileri seviye)
2. Ekranda maksimum engel sayısını sınırla
3. Engel yok etme pozisyonunu uygun ayarla
4. Basit collider'lar kullan (box/circle)

## Sonraki Adımlar

1. Geçici sprite'ları gerçek pixel art ile değiştir
2. Ses efektleri ekle (zıplama, çarpışma, skor)
3. Arka plan müziği ekle (8-bit buz mağarası teması)
4. Partikül efektleri ekle (kar, çarpışmada buz parçaları)
5. Parallax scrolling arka plan ekle
6. Farklı engel kombinasyonları oluştur
7. Power-up'lar ekle (yıldız, yavaşlatma)

---

## Dosya Yapısı

```
Assets/
├── Scenes/
│   └── Level4_PenguinRunner.unity
├── Scripts/
│   ├── PenguinController.cs
│   ├── ObstacleMovement.cs
│   ├── ObstacleSpawner.cs
│   ├── PenguinGameManager.cs
│   ├── LevelManager.cs (değiştirildi)
│   └── MenuManager.cs (değiştirildi)
├── Prefabs/
│   └── Obstacles/
│       ├── Stalagmite_Small.prefab
│       ├── Stalagmite_Medium.prefab
│       ├── Stalagmite_Large.prefab
│       ├── Stalactite_Short.prefab
│       ├── Stalactite_Long.prefab
│       └── Stalactite_VeryLong.prefab
├── Sprites/
│   └── PenguinRunner/ (opsiyonel)
│       ├── penguin_run.png
│       ├── penguin_jump.png
│       ├── penguin_duck.png
│       ├── stalagmite.png
│       └── stalactite.png
└── Animators/ (opsiyonel)
    └── PenguinAnimator.controller
```

## Oyun Kontrolü

- **W**: Zıpla (zemin engellerinden atla)
- **S**: Eğil (tavan engellerinden kaç)

## Notlar

- Bu oyun Google Chrome'un T-Rex Runner oyunundan esinlenmiştir
- E-health ciddi oyun projesi için buz mağarası penguen teması olarak uyarlanmıştır
- Zorluk sistemi Chrome Dino ile aynı formülü kullanır
- Skor otomatik kaydedilir (PlayerPrefs)

---

**Hazırlayan:** Claude Code Assistant
**Tarih:** 2025
**Proje:** E-health Serious Game - Ice Cave Runner (Level 5)
