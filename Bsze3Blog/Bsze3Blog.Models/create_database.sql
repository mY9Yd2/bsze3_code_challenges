-- Szerintem ez most nem sikerült annyira jól. Nekem nem igazán tetszik.
-- A törölt felhasználói állapot része okoz némi fejtörést számomra:
--
-- Ha törölt, akkor törölhetnénk a user_personal_informations (röviden: UPI) táblában lévő adatát.
-- Viszont a jelszavára és a régi jelszavára és még egy-két tábla adataira sem lenne már szükség
-- ezért azok mutathatnának az UPI táblára, hogy törléskor automatikusan törlődjenek
-- de így olyan furcsán nézne ki az összkép.
--
-- Egy másik dolog még: a created_at & updated_at oszlopok >
-- Ha sokféle oszlop van, akkor nem tudom mennyi hasznot hordoz.
-- Ha netán mindent külön számontartok (email, admin, mod, telephone..),
-- az pedig túlzásnak tűnik (pl. nem akarom, hogy óránként nevet változtasson vagy azt máshol kellene megvalósítani?).
--
-- Minden táblába rakni egy darab created_at, updated_at oszlopot pedig nem tudom mennyire jó ötlet,
-- bár a sequelize* épp alapból ilyesmit csinál, de az valószínűleg más célt szolgál
-- * https://sequelize.org/docs/v6/core-concepts/model-basics/#timestamps


DROP DATABASE IF EXISTS bsze3_blog;


CREATE DATABASE bsze3_blog
    CHARACTER SET = 'utf8mb4'
    COLLATE = 'utf8mb4_hungarian_ci';


USE bsze3_blog;


CREATE TABLE users (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    user_status ENUM('Active', 'Inactive', 'Deleted') NOT NULL
        DEFAULT 'Inactive' COMMENT 'A felhasználó jelenlegi állapota',
    created_at TIMESTAMP
        DEFAULT CURRENT_TIMESTAMP COMMENT 'A felhasználó létrehozásának időpontja',
    updated_at TIMESTAMP
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP
) COMMENT = 'Felhasználói fiókok & aktivitási állapota';


CREATE TABLE user_personal_informations (
    user_id INT UNSIGNED NOT NULL PRIMARY KEY,
    display_name VARCHAR(70) NOT NULL COMMENT 'A felhasználó neve, amit mások is láthatnak',
    email VARCHAR(40) NOT NULL UNIQUE COMMENT 'A felhasználó emailcíme',
    telephone_number CHAR(14) NULL COMMENT 'A felhasználó telefonszáma 00-00-000-0000 formátumban',
    created_at TIMESTAMP
        DEFAULT CURRENT_TIMESTAMP
        COMMENT 'A felhasználó személyes adatainak a létrehozásának időpontja',
    updated_at TIMESTAMP
        DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT chk_telephone_number_format
        CHECK (telephone_number REGEXP '\\d\\d-\\d\\d-\\d\\d\\d-\\d\\d\\d\\d'),
    CONSTRAINT fk_user_personal_informations__users
        FOREIGN KEY (user_id)
        REFERENCES users (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) COMMENT = 'A felhasználók személyes adatai';


CREATE TABLE permissions (
    user_id INT UNSIGNED NOT NULL PRIMARY KEY,
    can_create_blog BOOLEAN NOT NULL
        DEFAULT FALSE COMMENT 'A felhasználónak van-e jogosultsága létrehozni egy blogot',
    can_delete_blog BOOLEAN NOT NULL
        DEFAULT FALSE COMMENT 'A felhasználónak van-e jogosultsága törölni egy blogot',
    can_edit_blog BOOLEAN NOT NULL
        DEFAULT FALSE COMMENT 'A felhasználónak van-e jogosultsága szerkeszteni egy blogot',
    CONSTRAINT fk_permissions__users
        FOREIGN KEY (user_id)
        REFERENCES users (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) COMMENT = 'A felhasználók jogosultságai';


CREATE TABLE roles (
    user_id INT UNSIGNED NOT NULL PRIMARY KEY,
    is_administrator BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'Adminisztrátor-e a felhasználó',
    is_moderator BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'Moderátor-e a felhasználó',
    CONSTRAINT fk_roles__users
        FOREIGN KEY (user_id)
        REFERENCES users (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) COMMENT = 'A felhasználók jogosultsági szerepkörei';


CREATE TABLE passwords (
    user_id INT UNSIGNED NOT NULL PRIMARY KEY,
    password_hash VARCHAR(84) NOT NULL COMMENT 'A felhasználó hashelt jelszava',
    expire_at TIMESTAMP NOT NULL
        DEFAULT TIMESTAMPADD(YEAR, 3, CURRENT_TIMESTAMP)
        COMMENT 'A jelszó lejárati dátuma, ami után már nem érvényes',
    CONSTRAINT fk_passwords__users
        FOREIGN KEY (user_id)
        REFERENCES users (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) COMMENT = 'A felhasználók jelenlegi jelszavai';


CREATE TABLE old_passwords (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    password_hash VARCHAR(84) NOT NULL COMMENT 'A felhasználó hashelt jelszava',
    expired_at TIMESTAMP NOT NULL COMMENT 'A jelszó lejárati dátuma, ami után már nem érvényes',
    user_id INT UNSIGNED NOT NULL,
    CONSTRAINT fk_old_passwords__users
        FOREIGN KEY (user_id)
        REFERENCES users (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) COMMENT = 'A felhasználók lejárt, érvényét vesztett jelszavai';


CREATE TABLE last_logins (
    user_id INT UNSIGNED NOT NULL PRIMARY KEY,
    last_login_at TIMESTAMP NOT NULL
        DEFAULT CURRENT_TIMESTAMP COMMENT 'Az utolsó bejelentkezés időpontja',
    CONSTRAINT fk_last_logins__users
        FOREIGN KEY (user_id)
        REFERENCES users (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) COMMENT = 'Az utolsó sikeres bejelentkezések időpontjai a felhasználói fiókokba';


CREATE TABLE failed_login_attempts (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    user_id INT UNSIGNED NOT NULL,
    attempt_at TIMESTAMP NOT NULL
        DEFAULT CURRENT_TIMESTAMP COMMENT 'A bejelentkezési kísérlet időpontja',
    CONSTRAINT fk_failed_login_attempts__users
        FOREIGN KEY (user_id)
        REFERENCES users (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) COMMENT = 'Sikertelen bejelentkezési kísérletek időpontjai a felhasználói fiókokba';


CREATE TABLE blog_entries (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(70) NOT NULL COMMENT 'A blog címe',
    content TEXT NOT NULL COMMENT 'A blog tartalma',
    author INT UNSIGNED NOT NULL COMMENT 'A blogot létrehozó felhasználó (FK ID)',
    number_of_views INT UNSIGNED NOT NULL
        DEFAULT 0 COMMENT 'Hányszor tekintették meg a blogot a felhasználók',
    created_at TIMESTAMP
        DEFAULT CURRENT_TIMESTAMP COMMENT 'A blog létehozásának időpontja',
    deleted_at TIMESTAMP NULL COMMENT 'A blog "törlési" időpontja',
    CONSTRAINT fk_blog_entries__user_personal_informations
        FOREIGN KEY (author)
        REFERENCES user_personal_informations (user_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) COMMENT = 'Blog bejegyzések & alap adatai';


CREATE TABLE blog_views (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    blog_entry_id INT UNSIGNED NOT NULL,
    viewed_by INT UNSIGNED NULL COMMENT 'A felhasználó, aki megtekintette a blogot (FK ID)',
    viewed_at TIMESTAMP NOT NULL
        DEFAULT CURRENT_TIMESTAMP COMMENT 'Az időpont, amikor a felhasználó megtekintette a blogot',
    CONSTRAINT fk_blog_views__blog_entries
        FOREIGN KEY (blog_entry_id)
        REFERENCES blog_entries (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_blog_views__users
        FOREIGN KEY (viewed_by)
        REFERENCES users (id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
) COMMENT = 'A blogok megtekintés és látogatási adatai, időpontjai';


CREATE TABLE blog_edit_logs (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    blog_entry_id INT UNSIGNED NOT NULL,
    edited_by INT UNSIGNED NULL COMMENT 'A felhasználó, aki szerkesztette a blogot (FK ID)',
    edited_at TIMESTAMP NOT NULL
        DEFAULT CURRENT_TIMESTAMP COMMENT 'A blog szerkesztésének, módosításának időpontja',
    CONSTRAINT fk_blog_edit_logs__blog_entries
        FOREIGN KEY (blog_entry_id)
        REFERENCES blog_entries (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_blog_edit_logs__users
        FOREIGN KEY (edited_by)
        REFERENCES users (id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
) COMMENT = 'A blogok szerkesztési naplója';


DELIMITER //


CREATE PROCEDURE uspPreserveOldPasswordHash (
    password_hash VARCHAR(84),
    expired_at TIMESTAMP,
    user_id INT UNSIGNED
)
    MODIFIES SQL DATA
    COMMENT 'A felhasználó jelszavát beszúrja az old_passwords táblába'
BEGIN
    INSERT INTO old_passwords (password_hash, expired_at, user_id)
        VALUES (password_hash, expired_at, user_id);
END;//


CREATE PROCEDURE uspMarkUserAsActive (
    last_login_at TIMESTAMP,
    user_id INT UNSIGNED
)
    MODIFIES SQL DATA
    COMMENT 'Ha a felhasználó belépett az elmúlt két évben, akkor aktívnak számít'
BEGIN
    IF TIMESTAMPDIFF(YEAR, last_login_at, CURRENT_TIMESTAMP()) >= 2 THEN
        UPDATE users
            SET users.user_status = 'Inactive'
            WHERE users.id = user_id
                AND users.user_status != 'Deleted';
    ELSE
        UPDATE users
            SET users.user_status = 'Active'
            WHERE users.id = user_id
                AND users.user_status != 'Deleted';
    END IF;
END;//


CREATE TRIGGER TR_preserve_old_password_hash_BU
    BEFORE UPDATE
    ON passwords FOR EACH ROW
BEGIN
    IF NEW.password_hash <> OLD.password_hash THEN
        IF OLD.expire_at > CURRENT_TIMESTAMP() THEN
            -- Ha a lejárati idő előtt jelszót változtattunk,
            -- akkor a lejárati idő a mai dátum lesz
            CALL uspPreserveOldPasswordHash(OLD.password_hash, CURRENT_TIMESTAMP(), NEW.user_id);
        ELSE
            CALL uspPreserveOldPasswordHash(OLD.password_hash, OLD.expire_at, NEW.user_id);
        END IF;

        SET NEW.expire_at = TIMESTAMPADD(YEAR, 3, CURRENT_TIMESTAMP);
    END IF;
END;//


CREATE TRIGGER TR_preserve_old_password_hash_BD
    BEFORE DELETE
    ON passwords FOR EACH ROW
BEGIN
    IF OLD.expire_at > CURRENT_TIMESTAMP() THEN
        -- Ha a lejárati idő előtt jelszót törlünk,
        -- akkor a lejárati idő a mai dátum lesz
        CALL uspPreserveOldPasswordHash(OLD.password_hash, CURRENT_TIMESTAMP(), OLD.user_id);
    ELSE
        CALL uspPreserveOldPasswordHash(OLD.password_hash, OLD.expire_at, OLD.user_id);
    END IF;
END;//


CREATE TRIGGER TR_mark_user_as_active_BI
    BEFORE INSERT
    ON last_logins FOR EACH ROW
BEGIN
    CALL uspMarkUserAsActive(NEW.last_login_at, NEW.user_id);
END;//


CREATE TRIGGER TR_mark_user_as_active_BU
    BEFORE UPDATE
    ON last_logins FOR EACH ROW
BEGIN
    CALL uspMarkUserAsActive(NEW.last_login_at, NEW.user_id);
END;//


-- https://mariadb.com/docs/server/ref/mdb/system-variables/event_scheduler/
-- "its value will be reset the next time"
SET GLOBAL event_scheduler = ON;//


CREATE EVENT E_mark_user_as_inactive
    ON SCHEDULE
        EVERY 2 YEAR
    ON COMPLETION PRESERVE
    ENABLE
    COMMENT 'A felh. inaktív, ha nem lépett be legalább két éve'
DO
BEGIN
    UPDATE users
        LEFT JOIN last_logins
            ON last_logins.user_id = users.id
        SET users.user_status = 'Inactive'
        WHERE
            (TIMESTAMPDIFF(YEAR, last_logins.last_login_at, CURRENT_TIMESTAMP()) >= 2
            OR last_logins.last_login_at IS NULL)
            AND users.user_status != 'Deleted';
END;//


DELIMITER ;
