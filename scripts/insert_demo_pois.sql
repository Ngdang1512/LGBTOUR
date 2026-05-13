-- Insert Demo POIs for Trial App
-- Location: Ho Chi Minh City - District 1

-- 1. Nhà Thờ Đức Bà (Saigon Notre-Dame Cathedral)
INSERT INTO POIs (Name, Description, Lat, Lng, Radius, Priority, IsStopStation, Image)
VALUES (
    'Nhà Thờ Đức Bà',
    'Nhà thờ Đức Bà là một nhà thờ Công giáo La Mã nằm ở trung tâm thành phố Hồ Chí Minh, được xây dựng từ 1877-1883. Đây là một tác phẩm kiến trúc Pháp cổ điển vô cùng ấn tượng.',
    10.7829, 106.6982, 100, 100, 0, '/images/nha-tho-duc-ba.jpg'
);

-- 2. Dinh Độc Lập (Reunification Palace)
INSERT INTO POIs (Name, Description, Lat, Lng, Radius, Priority, IsStopStation, Image)
VALUES (
    'Dinh Độc Lập',
    'Dinh Độc Lập, còn được gọi là Dinh Tổng Thống, là một điểm đến lịch sử quan trọng tại Sài Gòn. Đây là nơi Thủ tướng Phạm Văn Đồng đã sống và làm việc.',
    10.7920, 106.6868, 100, 95, 0, '/images/dinh-doc-lap.jpg'
);

-- 3. Bến Nhà Rồng (Dragon House Wharf)
INSERT INTO POIs (Name, Description, Lat, Lng, Radius, Priority, IsStopStation, Image)
VALUES (
    'Bến Nhà Rồng',
    'Bến Nhà Rồng là một công trình kiến trúc cổ kính bên bờ sông Sài Gòn, được xây dựng vào thế kỷ 19. Nơi đây đã chứng kiến nhiều sự kiện lịch sử quan trọng của Việt Nam.',
    10.7627, 106.6881, 80, 90, 1, '/images/ben-nha-rong.jpg'
);

-- 4. Chợ Bến Thành (Ben Thanh Market)
INSERT INTO POIs (Name, Description, Lat, Lng, Radius, Priority, IsStopStation, Image)
VALUES (
    'Chợ Bến Thành',
    'Chợ Bến Thành là một chợ truyền thống nổi tiếng ở Sài Gòn, nơi du khách có thể tìm thấy hàng hóa địa phương, đồ lưu niệm và đặc sản Việt Nam.',
    10.7720, 106.6967, 100, 85, 1, '/images/cho-ben-thanh.jpg'
);

-- 5. Tòa Nhà Bitexco (Bitexco Financial Tower)
INSERT INTO POIs (Name, Description, Lat, Lng, Radius, Priority, IsStopStation, Image)
VALUES (
    'Tòa Nhà Bitexco',
    'Tòa Nhà Bitexco Financial Tower là một tòa nhà cao ốc hiện đại, được hoàn thành vào năm 2010. Từ tầng 49 Sky Garden, du khách có thể ngắm toàn cảnh Sài Gòn từ độ cao 203 mét.',
    10.7614, 106.7244, 100, 80, 0, '/images/bitexco-tower.jpg'
);

-- Print confirmation
SELECT 'Demo POIs inserted successfully!' AS Message;
SELECT COUNT(*) AS TotalPOIs FROM POIs;
