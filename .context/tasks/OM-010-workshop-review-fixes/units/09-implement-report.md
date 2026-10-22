# Unit 09 report

Updated the REPAIR_ON_LOAD.TOOLTIP msgid in the .pot, ko, ru and zh .po files, and the three translations:

- ko: 액체 벤트가 생성된 뒤(불러올 때 또는 건설될 때) 첫 1초 동안, 벤트 위치나 그 아래에 ...
- ru: В первую секунду после появления клапана для жидкости (при загрузке или при постройке) природная клетка, ...
- zh: 液体排液口生成后（加载时或建造时）的第一秒内，若排液口处或其下方 ...

Check output (`python common/tools/Translations/translations.py mods/vent-freeze-fix --build`):

```
Wrote 13 strings to ...VentFreezeFix.pot
ko.po: 13/13 translated
ru.po: 13/13 translated
zh.po: 13/13 translated
ALL CHECKS PASSED
```
