# Claude-game- (لعبة رعب - Stride Engine)

بيت مهجور مظلم، كشاف فقط، ووحش يتجول. اجمع 5 مفاتيح واهرب من الباب الأخضر.

## التحكم
WASD حركة | الماوس نظر | Shift جري (يجذب الوحش) | F كشاف | E اختباء في الدولاب | R إعادة/المستوى التالي

## القواعد
- الوحش يسمع الجري من 14 م والمشي من 8 م.
- بطارية الكشاف تنقص تدريجياً، والكشاف يومض عند اقتراب الوحش.
- بطاريتان زرقاوان في البيت تعبّيان الكشاف (+40%).
- دولابان للاختباء (E): إذا بقي الوحش قريباً منك أكثر من 2.5 ثانية يكتشفك.
- الوحش يمر عبر الأبواب، ويتذكر آخر مكان سمعك فيه ويذهب إليه.
- كل مرة تهرب يبدأ مستوى جديد والوحش أسرع بـ 15%.
- نبض القلب يعلو كلما اقترب الوحش، وإذا مسكك تلتفت الكاميرا له (jumpscare).

## التشغيل
1. أنشئ مشروع Stride (قالب New Game) على Windows.
2. احذف Camera و Directional light و Skybox من المشهد.
3. أضف `Scripts/HorrorGame.cs` كـ Script على Entity فارغ.
4. (اختياري) استورد ملفات `Assets/Audio/*.wav` بأسماء `drone` و`heartbeat` و`step` و`pickup` و`scream` ليشتغل الصوت.
5. (اختياري) استورد `Assets/Models/Generated/Monster.obj` باسم أصل `Monster` ليظهر الوحش بدل المهرج.
6. F5.

## الموديلات
- `Assets/Models/Generated`: مولّدة بالكود (`tools/generate_models.py`): Clown, House, Key, ExitDoor, Monster.
- `Assets/Models/CC0`: حزم KayKit (Furniture, Halloween, Dungeon) و CC0Tree، رخصة CC0.
