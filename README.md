# Claude-game- (لعبة رعب - Stride Engine)

بيت مهجور مظلم، كشاف فقط، ووحش يتجول. اجمع 5 مفاتيح واهرب من الباب الأخضر.

## التحكم
WASD حركة | الماوس نظر | Shift جري (يجذب الوحش) | F كشاف | R إعادة

## القواعد
- الوحش يسمع الجري من 14 م والمشي من 8 م.
- بطارية الكشاف تنقص تدريجياً، والكشاف يومض عند اقتراب الوحش.
- إذا مسكك الوحش تخسر.

## التشغيل
1. أنشئ مشروع Stride (قالب New Game) على Windows.
2. احذف Camera و Directional light و Skybox من المشهد.
3. أضف `Scripts/HorrorGame.cs` كـ Script على Entity فارغ.
4. (اختياري) استورد `Assets/Models/Generated/Monster.obj` باسم أصل `Monster` ليظهر الوحش بدل المهرج.
5. F5.

## الموديلات
- `Assets/Models/Generated`: مولّدة بالكود (`tools/generate_models.py`): Clown, House, Key, ExitDoor, Monster.
- `Assets/Models/CC0`: حزم KayKit (Furniture, Halloween, Dungeon) و CC0Tree، رخصة CC0.
