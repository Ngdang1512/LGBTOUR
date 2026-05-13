-- Insert Demo Narrations for Trial POIs

-- Nhà Thờ Đức Bà - Vietnamese
INSERT INTO Narrations (PoiId, Language, ScriptText, AudioUrl, CreatedAt)
SELECT Id, 'vi', 
'Xin chào các bạn, bây giờ chúng ta đang tới Nhà Thờ Đức Bà, một trong những công trình kiến trúc tôn giáo đẹp nhất ở Sài Gòn. Nhà thờ này được xây dựng từ 1877 đến 1883 bởi người Pháp, với phong cách kiến trúc Phục Hưng Ý. Mặt tiền của nhà thờ nổi bật với hai tòa tháp cao vút lên trời, được trang trí bằng những chân dung các vị thánh. Bên trong, bạn sẽ thấy những cửa kính màu tuyệt đẹp và tác phẩm nghệ thuật tôn giáo quý hiếm. Đây là một nơi yên tĩnh, linh thiêng, và là điểm đến phải ghé thăm khi du lịch Sài Gòn.',
NULL, GETDATE()
FROM POIs WHERE Name = N'Nhà Thờ Đức Bà';

-- Nhà Thờ Đức Bà - English
INSERT INTO Narrations (PoiId, Language, ScriptText, AudioUrl, CreatedAt)
SELECT Id, 'en',
'Welcome to the Saigon Notre-Dame Cathedral, one of the most beautiful religious architectural works in Ho Chi Minh City. This church was built from 1877 to 1883 by the French, with Italian Renaissance architecture style. The facade of the church stands out with two towering spires reaching to the sky, decorated with portraits of saints. Inside, you will see beautiful stained glass windows and rare religious artworks. This is a quiet, sacred place, and a must-visit destination when traveling in Saigon.',
NULL, GETDATE()
FROM POIs WHERE Name = N'Nhà Thờ Đức Bà';

-- Dinh Độc Lập - Vietnamese
INSERT INTO Narrations (PoiId, Language, ScriptText, AudioUrl, CreatedAt)
SELECT Id, 'vi',
'Chúng ta đang ghé thăm Dinh Độc Lập, một công trình kiến trúc và di tích lịch sử quan trọng. Tòa nhà này được xây dựng vào năm 1868 bởi người Pháp với kiến trúc Pháp cổ điển. Nó từng là nơi ở và làm việc của các Tổng Thống miền Nam Việt Nam. Hôm nay, Dinh Độc Lập đã trở thành một bảo tàng, nơi bạn có thể tìm hiểu về lịch sử Việt Nam thông qua các bản đồ, hình ảnh và tài liệu lịch sử.',
NULL, GETDATE()
FROM POIs WHERE Name = N'Dinh Độc Lập';

-- Dinh Độc Lập - English
INSERT INTO Narrations (PoiId, Language, ScriptText, AudioUrl, CreatedAt)
SELECT Id, 'en',
'We are visiting the Reunification Palace, an important architectural work and historical monument. This building was constructed in 1868 by the French with classical French architecture. It was once the residence and workplace of the Presidents of South Vietnam. Today, the Reunification Palace has become a museum where you can learn about Vietnamese history through maps, photographs and historical documents.',
NULL, GETDATE()
FROM POIs WHERE Name = N'Dinh Độc Lập';

-- Bến Nhà Rồng - Vietnamese
INSERT INTO Narrations (PoiId, Language, ScriptText, AudioUrl, CreatedAt)
SELECT Id, 'vi',
'Đây là Bến Nhà Rồng, một công trình kiến trúc cổ kính bên bờ sông Sài Gòn. Bến này được xây dựng vào thế kỷ 19 và đã chứng kiến nhiều sự kiện lịch sử quan trọng của Việt Nam. Tên gọi Nhà Rồng xuất phát từ hai con rồng được chạm khắc trên mái của tòa nhà. Nó từng là một bến tàu quan trọng cho giao dịch quốc tế. Hôm nay, Bến Nhà Rồng đã trở thành một bảo tàng, nơi trưng bày những tác phẩm nghệ thuật và hiện vật lịch sử.',
NULL, GETDATE()
FROM POIs WHERE Name = N'Bến Nhà Rồng';

-- Bến Nhà Rồng - English
INSERT INTO Narrations (PoiId, Language, ScriptText, AudioUrl, CreatedAt)
SELECT Id, 'en',
'This is the Dragon House Wharf, an ancient architectural structure on the Saigon River bank. This wharf was built in the 19th century and witnessed many important historical events in Vietnam. The name Dragon House comes from two dragons carved on the roof of the building. It was once an important port for international trade. Today, the Dragon House Wharf has become a museum displaying artworks and historical artifacts.',
NULL, GETDATE()
FROM POIs WHERE Name = N'Bến Nhà Rồng';

-- Print confirmation
SELECT 'Demo Narrations inserted successfully!' AS Message;
SELECT COUNT(*) AS TotalNarrations FROM Narrations;
