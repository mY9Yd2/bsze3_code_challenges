-- Angol oszlop neveket választottam, aztán remélem a megfelelő szót is

DROP DATABASE IF EXISTS bsze3;

CREATE DATABASE bsze3
    CHARACTER SET = 'utf8mb4'
    COLLATE = 'utf8mb4_hungarian_ci';

USE bsze3;

CREATE TABLE students (
    education_id CHAR(11) NOT NULL PRIMARY KEY,
    full_name VARCHAR(70) NOT NULL,
    birthdate DATE NOT NULL,
    age TINYINT UNSIGNED
        AS (TIMESTAMPDIFF(YEAR, birthdate, CURDATE())),
    sex ENUM('Male', 'Female') NULL,
    place_of_residence VARCHAR(128) NOT NULL,
    class VARCHAR(18) NOT NULL
);

CREATE TABLE professions (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    profession VARCHAR(128) NOT NULL UNIQUE
);

CREATE TABLE students_professions (
    student_id CHAR(11) NOT NULL,
    profession_id INT UNSIGNED NOT NULL,
    PRIMARY KEY (student_id, profession_id),
    CONSTRAINT fk_profession_student
        FOREIGN KEY (student_id)
        REFERENCES students (education_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_student_profession
        FOREIGN KEY (profession_id)
        REFERENCES professions (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

CREATE TABLE subjects (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    school_subject VARCHAR(128) NOT NULL UNIQUE
);

CREATE TABLE grades (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    grade TINYINT NOT NULL,
    subject_id INT UNSIGNED NULL,
    student_id CHAR(11) NOT NULL,
    CONSTRAINT chk_grade
        CHECK (grade >= 1 AND grade <= 5),
    CONSTRAINT fk_grade_subject
        FOREIGN KEY (subject_id)
        REFERENCES subjects (id)
        ON DELETE SET NULL
        ON UPDATE CASCADE,
    CONSTRAINT fk_grade_student
        FOREIGN KEY (student_id)
        REFERENCES students (education_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

CREATE TABLE absences (
    absence_date DATE NOT NULL,
    proven BOOLEAN NOT NULL DEFAULT FALSE,
    class TINYINT NOT NULL,
    subject_id INT UNSIGNED NULL,
    student_id CHAR(11) NOT NULL,
    PRIMARY KEY (absence_date, class, student_id),
    CONSTRAINT chk_class
        CHECK (class >= 0 AND class <= 15),
    CONSTRAINT fk_absence_subject
        FOREIGN KEY (subject_id)
        REFERENCES subjects (id)
        ON DELETE SET NULL
        ON UPDATE CASCADE,
    CONSTRAINT fk_absence_student
        FOREIGN KEY (student_id)
        REFERENCES students (education_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

CREATE TABLE topics (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    topic VARCHAR(128) NOT NULL UNIQUE,
    subject_id INT UNSIGNED NULL,
    CONSTRAINT fk_topic_subject
        FOREIGN KEY (subject_id)
        REFERENCES subjects (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

CREATE TABLE kahoot_tests (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    number_of_questions TINYINT UNSIGNED NOT NULL,
    started_at DATETIME NOT NULL DEFAULT NOW(),
    subject_id INT UNSIGNED NULL,
    topic_id INT UNSIGNED NULL,
    CONSTRAINT chk_number_of_questions
        CHECK (number_of_questions > 0),
    CONSTRAINT fk_kahoot_subject
        FOREIGN KEY (subject_id)
        REFERENCES subjects (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_kahoot_topic
        FOREIGN KEY (topic_id)
        REFERENCES topics (id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);

CREATE TABLE kahoot_test_results (
    score MEDIUMINT UNSIGNED NOT NULL,
    number_of_failed_responses TINYINT UNSIGNED NOT NULL,
    place TINYINT UNSIGNED NOT NULL,
    student_id CHAR(11) NULL,
    kahoot_test_id INT UNSIGNED NOT NULL,
    PRIMARY KEY(place, kahoot_test_id),
    CONSTRAINT chk_place
        CHECK (place > 0),
    CONSTRAINT fk_kahoot_result_student
        FOREIGN KEY (student_id)
        REFERENCES students (education_id)
        ON DELETE SET NULL
        ON UPDATE CASCADE,
    CONSTRAINT fk_kahoot_result_kahoot_test
        FOREIGN KEY (kahoot_test_id)
        REFERENCES kahoot_tests (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
