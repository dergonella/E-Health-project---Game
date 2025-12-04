# 🚀 Penguin Runner - HIZLI BAŞLANGIÇ

## ⚠️ ÖNEMLİ - İLK ADIM

**Unity'de LevelManager'ı güncelleyin:**

1. Unity'yi kapatın (dosyalar kilitli olmasın)
2. `Assets/Scripts/LevelManager.cs` dosyasını silin
3. `Assets/Scripts/LevelManager_UPDATED.cs` dosyasını `LevelManager.cs` olarak yeniden adlandırın
4. Unity'yi açın

## 📦 Hazır Dosyalar

✅ Tüm script'ler `F:/Unity/E-Health-project---Game/Assets/Scripts/` klasöründe:
- `RunnerPlayerController.cs`
- `RunnerGameManager.cs`
- `RunnerObstacleSpawner.cs`
- `RunnerObstacle.cs`
- `RunnerGroundScroller.cs`

## 🎯 5 Dakikada Çalışan Oyun

### 1. Scene Oluştur (1 dk)
```
File > New Scene
File > Save As: Assets/Scenes/RunnerLevel.unity
```

### 2. GameManager (30 sn)
```
GameObject > Create Empty > "GameManager"
Add Component > RunnerGameManager
Add Component > Audio Source
```

### 3. Player Oluştur (1 dk)
```
GameObject > 2D Object > Sprite > "Player"
Position: (-6, -2, 0)
Scale: (0.5, 0.5, 1)

Add Components:
- RunnerPlayerController
- Rigidbody2D (Gravity: 0, Kinematic)
- BoxCollider2D x2 (biri standing, biri ducking)

Tag: Player
```

**PlayerController Ayarları:**
- Ground Y: `-2`
- Jump Velocity: `12`
- Gravity: `0.6`
- Standing/Ducking Collider'ları sürükle

### 4. Zemin (1 dk)
```
GameObject > 3D Object > Quad > "Ground"
Position: (0, -3, 0)
Scale: (20, 1, 1)
Rotation: (90, 0, 0)

Add Component:
- BoxCollider2D (Is Trigger: FALSE!)
- RunnerGroundScroller
```

### 5. Obstacle Spawner (30 sn)
```
GameObject > Create Empty > "ObstacleSpawner"
Add Component > RunnerObstacleSpawner

Settings:
- Spawn X: 12
- Gap Coefficient: 0.6
- Clear Time: 3
```

### 6. Engel Prefab (1 dk)
```
GameObject > 2D Object > Sprite > "Obstacle"
Position: (10, -2, 0)
Scale: (0.5, 1, 1)

Add Components:
- BoxCollider2D (Is Trigger: TRUE!)
- RunnerObstacle

Tag: Obstacle

Project'e sürükle (prefab yap)
Sahneden sil
```

**ObstacleSpawner'a ekle:**
- Obstacle Types: Size = 1
- Element 0:
  - Name: "Basic"
  - Prefab: Obstacle prefab'ını sürükle
  - Width: 1
  - Min Gap: 3

### 7. UI (1 dk)
```
GameObject > UI > Canvas

Canvas altında:
- Text (TMP): "ScoreText" - Top Right (-100, -50)
- Text (TMP): "HighScoreText" - Top Right (-100, -100)
- Panel: "GameOverPanel"
  - Text: "GAME OVER"
  - Button: "Restart"
  - Button: "Menu"
```

**GameManager'a bağla:**
- UI referanslarını sürükle
- Butonları bağla

### 8. Build Settings (30 sn)
```
File > Build Settings
Add Open Scenes
```

### 9. TEST! 🎮
Play'e bas ve test et:
- Space ile zıpla
- S ile eğil
- Engellere çarpma!

## 🎨 Basit Sprite Oluşturma

Hızlı test için:

**Player Sprite:**
```
Assets > Create > Sprites > Square
Renk: Mavi/Yeşil
```

**Obstacle Sprite:**
```
Assets > Create > Sprites > Square
Renk: Kırmızı
```

**Ground:**
```
Assets > Create > Material
Color: Gri
Tiling: (10, 1)
```

## 🐛 Hızlı Hatalar

| Hata | Çözüm |
|------|-------|
| Player düşüyor | Rigidbody2D > Gravity: 0 yap |
| Engeller spawn olmuyor | ObstacleSpawner > Spawn X: 12 |
| Collision çalışmıyor | Tag'leri kontrol et! |
| Zemin hareket etmiyor | RunnerGroundScroller ekle |
| Menu'de Level 5 yok | LevelManager'ı güncelle! |

## 🎯 Level Manager Güncelleme

**MANUEL YOL:**
1. Unity'yi KAPAT
2. `LevelManager.cs` dosyasında:
   - Satır 29: `new LevelData[4]` → `new LevelData[5]`
   - Satır 113'ten sonra Level 5'i ekle (LevelManager_UPDATED.cs'den kopyala)
3. Unity'yi AÇ

**OTOMATIK YOL:**
1. Unity'yi KAPAT
2. `LevelManager.cs` → sil
3. `LevelManager_UPDATED.cs` → `LevelManager.cs` olarak yeniden adlandır
4. Unity'yi AÇ

## 🎮 Kontroller

- **W / Space / Up Arrow**: Zıpla
- **S / Down Arrow**: Eğil
- **Enter (Game Over)**: Yeniden başlat

## 📋 Hızlı Checklist

- [ ] Scene oluşturuldu: `RunnerLevel.unity`
- [ ] GameManager + Audio Source eklendi
- [ ] Player + 2 collider + script eklendi
- [ ] Ground + collider + scroller eklendi
- [ ] ObstacleSpawner + 1 prefab hazır
- [ ] UI: Score + GameOver panel hazır
- [ ] GameManager referansları bağlandı
- [ ] LevelManager güncellendi (4→5)
- [ ] Build Settings'e eklendi
- [ ] TEST EDİLDİ ✅

## 💡 Sorun mu Var?

### Script Hatası
```
Assets/Scripts/ klasörüne bakın
Tüm Runner*.cs dosyaları orada mı?
```

### Level Menu'de Görünmüyor
```
LevelManager.cs'i güncelle
levels = new LevelData[5] olmalı
```

### Collision Çalışmıyor
```
Player: Tag = "Player"
Obstacle: Tag = "Obstacle"
Ground: Collider is NOT trigger
Obstacle: Collider IS trigger
```

## 🚀 Şimdi Ne Yapalım?

1. ✅ **Oyunu test et** - Çalışıyor mu?
2. 🎨 **Sprite'ları iyileştir** - Penguen/bira şişesi ekle
3. 🎵 **Ses ekle** - Jump, hit, score soundları
4. 🌟 **Daha fazla engel** - Farklı boyut ve tipler
5. 💎 **Powerup** - Coin sistemi

## 📚 Detaylı Bilgi

Detaylı kurulum için: `RUNNER_LEVEL_SETUP.md`

---

**Hazır! Şimdi oyunu test edebilirsin!** 🎮🐧🍺
