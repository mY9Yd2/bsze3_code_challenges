-- ki az az aktív felhasználó, akinek a legtöbb blog bejegyzése van
SELECT be.author
FROM blog_entries be
INNER JOIN users u
    ON
        u.user_status = 'Active'
        AND be.author = u.id
GROUP BY be.author
HAVING COUNT(be.author) = (
    -- max
    SELECT COUNT(be.author) AS c
    FROM blog_entries be
    INNER JOIN users u
        ON
            u.user_status = 'Active'
            AND be.author = u.id
    GROUP BY be.author
    ORDER BY c DESC
    LIMIT 1
);


-- soroljátok fel azokat az adminisztrátori jogosultsággal rendelkező felhasználókat,
-- akiknek nincs blog bejegyzése és írassátok mellete ki, hogy
-- az utolsó bejelntkezése óta mennyi idő telt el órában és percben lebontva
SELECT
    u.id AS user_id,
    TIMESTAMPDIFF(HOUR, ll.last_login_at, CURRENT_TIMESTAMP()) AS hours,
    TIMESTAMPDIFF(MINUTE, ll.last_login_at, CURRENT_TIMESTAMP())
        - TIMESTAMPDIFF(HOUR, ll.last_login_at, CURRENT_TIMESTAMP())
        * 60 AS minutes
FROM users u
INNER JOIN roles r
    ON
        r.is_administrator IS TRUE
        AND u.id = r.user_id
INNER JOIN last_logins ll
    ON u.id = ll.user_id
LEFT JOIN blog_entries be
    ON u.id = be.author
WHERE be.author IS NULL;


-- hány olyan felhasználó van, akinek 30 napon belül le fog járni a jelszava
SELECT COUNT(1)
FROM users u
INNER JOIN passwords p
    ON u.id = p.user_id
WHERE
    TIMESTAMPDIFF(SECOND, CURRENT_TIMESTAMP(), p.expire_at) <= 2.592 * POW(10, 6) -- 30 nap, másodpercben
    AND TIMESTAMPDIFF(SECOND, CURRENT_TIMESTAMP(), p.expire_at) > 0; -- akinek már lejárt a jelszava, az nem kell


-- kinek van vodafonos telefonszáma azok közül, akik moderátori jogosultsággal rendelkeznek
SELECT upi.user_id
FROM user_personal_informations upi
INNER JOIN roles r
    ON	r.is_moderator IS TRUE
        AND upi.user_id = r.user_id
WHERE upi.telephone_number LIKE '__-70-%';


-- kinek nincs semmilyen jogosultsága, azok közül, akik az összes blogbejegyzést megtekintették
SELECT r.user_id
FROM roles r
INNER JOIN blog_views bv
    ON r.user_id = bv.viewed_by
INNER JOIN permissions p
    ON r.user_id = p.user_id
WHERE
    is_administrator IS FALSE
    AND is_moderator IS FALSE
    AND p.can_create_blog IS FALSE
    AND p.can_delete_blog IS FALSE
    AND p.can_edit_blog IS FALSE
GROUP BY r.user_id
HAVING COUNT(DISTINCT bv.blog_entry_id) = (
    SELECT COUNT(1)
    FROM blog_entries be
);


-- listázd ki azokat a blogbejegyzéseket, amelyeknek a címe
-- egy-az-egyben megtalálható a szövegében is (kis-nagybetű ne számítson, bárhogy lehet írva)
SELECT be.*
FROM blog_entries be
WHERE be.content REGEXP CONCAT('\\b', be.title, '\\b');


-- kinek hány napja (csak egész számként jelenjen meg) van vissza a jelszavának lejáratáig
SELECT p.user_id, TIMESTAMPDIFF(DAY, CURRENT_TIMESTAMP(), p.expire_at) AS time_left
FROM passwords p
WHERE TIMESTAMPDIFF(SECOND, CURRENT_TIMESTAMP(), p.expire_at) > 0; -- akinek már lejárt a jelszava, az nem kell


-- ki hányszor változtatott eddig jelszót és mi a jelenlegi jelszava
SELECT p.user_id, COUNT(op.password_hash) AS c, p.password_hash
FROM old_passwords op
RIGHT JOIN passwords p
    ON op.user_id = p.user_id
GROUP BY p.user_id
