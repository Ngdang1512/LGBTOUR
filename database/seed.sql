-- ============================================================
-- Seed Data — Dev Environment
-- SaigonAudioTour
-- Chạy SAU khi đã apply 001_init.sql
-- ============================================================

-- ── Roles ──
INSERT OR IGNORE INTO Roles (Id, Name, Description) VALUES
  ('role-super', 'SuperAdmin', 'Toàn quyền hệ thống'),
  ('role-editor', 'Editor',    'Quản lý nội dung POI, Tour, Narration'),
  ('role-viewer', 'Viewer',    'Chỉ xem dashboard và báo cáo');

INSERT OR IGNORE INTO RolePermissions (RoleId, Permission) VALUES
  ('role-super',  'pois:write'),
  ('role-super',  'tours:write'),
  ('role-super',  'narrations:write'),
  ('role-super',  'analytics:read'),
  ('role-super',  'users:manage'),
  ('role-editor', 'pois:write'),
  ('role-editor', 'tours:write'),
  ('role-editor', 'narrations:write'),
  ('role-viewer', 'analytics:read');

-- ── Admin accounts (password: Admin@123) ──
-- BCrypt hash của "Admin@123" (work factor 12)
INSERT OR IGNORE INTO Admins (Id, Username, Email, PasswordHash, RoleId) VALUES
  ('admin-001', 'superadmin', 'super@saigontour.vn',
   '$2a$12$placeholder_hash_superadmin', 'role-super'),
  ('admin-002', 'editor1', 'editor@saigontour.vn',
   '$2a$12$placeholder_hash_editor1', 'role-editor');

-- ── Subscription Plans ──
INSERT OR IGNORE INTO SubscriptionPlans (Id, Name, Price, DurationDays, Description) VALUES
  ('plan-1m',  'Gói 1 Tháng',  49000,  30,  'Truy cập không giới hạn trong 1 tháng'),
  ('plan-3m',  'Gói 3 Tháng', 119000,  90,  'Tiết kiệm 19% so với gói tháng'),
  ('plan-12m', 'Gói 1 Năm',   399000, 365, 'Tiết kiệm 32% — tốt nhất cho du khách thường xuyên');

-- ── POIs — Quận 1, TP.HCM ──
INSERT OR IGNORE INTO POIs (Id, Name, Description, Latitude, Longitude, RadiusMeters, Priority, IsStopStation) VALUES
  ('poi-001', 'Nhà thờ Đức Bà',
   'Công trình kiến trúc Pháp nổi tiếng, xây dựng 1863–1880. Một trong những biểu tượng của TP.HCM.',
   10.7797, 106.6990, 80, 1, 1),

  ('poi-002', 'Bưu điện Trung tâm Sài Gòn',
   'Tòa nhà bưu điện lịch sử do kiến trúc sư Gustave Eiffel thiết kế, hoàn thành năm 1891.',
   10.7796, 106.6997, 70, 2, 0),

  ('poi-003', 'Dinh Độc Lập',
   'Dinh thự lịch sử, nơi diễn ra sự kiện thống nhất đất nước 30/4/1975. Hiện là di tích quốc gia.',
   10.7769, 106.6956, 100, 1, 1),

  ('poi-004', 'Bảo tàng Chứng tích Chiến tranh',
   'Bảo tàng lưu giữ hiện vật về cuộc kháng chiến, thu hút hàng triệu khách mỗi năm.',
   10.7796, 106.6921, 80, 2, 0),

  ('poi-005', 'Chợ Bến Thành',
   'Biểu tượng thương mại của Sài Gòn từ thế kỷ 20. Trung tâm mua sắm và ẩm thực nổi tiếng.',
   10.7722, 106.6983, 100, 1, 1),

  ('poi-006', 'Phố đi bộ Nguyễn Huệ',
   'Tuyến phố đi bộ dài 670m, là điểm tụ họp văn hóa và sự kiện lớn của thành phố.',
   10.7741, 106.7032, 90, 2, 0),

  ('poi-007', 'Nhà hát Thành phố',
   'Nhà hát Opera phong cách Pháp, xây năm 1900. Tọa lạc cuối đường Nguyễn Huệ.',
   10.7762, 106.7030, 70, 2, 0),

  ('poi-008', 'Bảo tàng TP. Hồ Chí Minh',
   'Tòa nhà mang dấu ấn Pháp, từng là Dinh Gia Long. Trưng bày lịch sử phát triển thành phố.',
   10.7773, 106.7002, 80, 3, 0),

  ('poi-009', 'Chùa Vĩnh Nghiêm',
   'Ngôi chùa lớn nhất Sài Gòn, kiến trúc kết hợp Nhật Bản và Việt Nam, xây dựng 1964.',
   10.7866, 106.6838, 90, 3, 0),

  ('poi-010', 'Công viên 30/4',
   'Công viên lịch sử đối diện Dinh Độc Lập, nơi diễn ra nhiều sự kiện văn hóa lớn.',
   10.7780, 106.6960, 80, 3, 0);

-- ── Tours ──
INSERT OR IGNORE INTO Tours (Id, Name, Description, EstimatedMinutes, TotalDistanceKm, TicketPrice) VALUES
  ('tour-001', 'Tour Quận 1 Cổ Điển',
   'Khám phá những công trình kiến trúc Pháp cổ điển và lịch sử trung tâm Sài Gòn.',
   90, 5.2, 150000),

  ('tour-002', 'Tour Lịch Sử & Văn Hóa',
   'Hành trình qua các địa điểm lịch sử quan trọng của TP.HCM — từ thời Pháp thuộc đến thống nhất.',
   120, 7.8, 180000);

-- ── Tour POIs ──
INSERT OR IGNORE INTO TourPOIs (TourId, POIId, OrderIndex) VALUES
  ('tour-001', 'poi-005', 0),
  ('tour-001', 'poi-006', 1),
  ('tour-001', 'poi-007', 2),
  ('tour-001', 'poi-001', 3),
  ('tour-001', 'poi-002', 4),

  ('tour-002', 'poi-003', 0),
  ('tour-002', 'poi-004', 1),
  ('tour-002', 'poi-008', 2),
  ('tour-002', 'poi-001', 3),
  ('tour-002', 'poi-010', 4);
