#!/usr/bin/env python3

import random
import time
import math
import string

from data import Data
from generate import Generate

import mysql.connector

STUDENTS_MAX = 39 * 20
KAHOOT_TESTS_MAX = 50

fix_male_full_names = [
    "Ajtai Zsolt Dávid",
    "Erdélyi Dávid",
    "Gajda Mihály Bence",
    "Horváth Péter",
    "Kaszab András",
    "Kiss Csongor",
    "Kovács Dániel",
    "Kovács József Miklós",
    "Kovács Krisztián József",
    "Molnár Ferenc Manó",
    "Székely Levente",
    "Ritter Csaba Zoltán",
    "Szathmári Jenő",
    "Varga Gábor",
    "Agatic Alex",
    "Balogh Ádám",
    "Csóti István",
    "Dombi Balázs",
    "Grezsa Zoltán Gábor",
    "Kopincu Máté",
    "Lajkó Levente",
    "Oláh Zsolt István",
    "Ördög Krisztofer Jenő",
    "Patócs Andor",
    "Simon Gábor",
    "Szalontai Péter",
    "Sztankovics Alex",
    "Takács András",
    "Töröcsik Ákos",
    "Vörös Ferenc",
    "Wittmann Gyula",
]
fix_female_full_names = [
    "Csernus Fruzsina",
    "Huszka Leila Renáta",
    "Pécsi Bianka",
    "Zsótér Kata",
    "Fekete Rita",
    "Kónya Brigitta",
    "Kramer Fanni Júlia",
    "Márkus Netti Teodóra",
]


def main() -> None:
    with open(".dbpass", "r", encoding="utf-8") as f:
        db_pass = f.readline().strip()

    con = mysql.connector.connect(
        host="localhost",
        user="root",
        password=db_pass,
    )

    with open("bsze3_kovjoz.sql", "r", encoding="utf-8") as f:
        with con.cursor() as c:
            c.execute(f.read(), multi=True)
        con.close()

    # TODO: UGLY workaround. Because it takes some time to create the tables.
    print("Sleep..", end="", flush=True)
    time.sleep(5)
    print(" ended")

    con.config(database="bsze3")
    con.connect()
    c = con.cursor()

    sql = "INSERT INTO subjects (school_subject) VALUES (%s)"
    c.executemany(sql, [(subject["name"],) for subject in Data.subjects])
    con.commit()

    for n_subject in Data.subjects:
        for n_topic in n_subject["topics"]:
            c.execute(
                "SELECT id FROM subjects WHERE school_subject = %s",
                (n_subject["name"],),
            )
            (subject_id,) = c.fetchone()

            c.execute(
                "INSERT INTO topics (topic, subject_id) VALUES (%s, %s)",
                (n_topic, subject_id),
            )
            con.commit()

    class_name_idx = 0
    for n_student in range(STUDENTS_MAX):
        education_id = Generate.education_id()

        if n_student > 0 and n_student % 39 == 0:
            class_name_idx += 1

        if fix_female_full_names:
            class_name = "1/13.be szoft"
            sex = "Female"
            full_name = fix_female_full_names.pop()
        elif fix_male_full_names:
            class_name = "1/13.be szoft"
            sex = "Male"
            full_name = fix_male_full_names.pop()
        else:
            class_name = f"1/13{string.ascii_lowercase[class_name_idx]}.be szoft"
            sex = Generate.sex()
            full_name = Generate.full_name(sex)

        birthdate = Generate.birthdate()
        place_of_residence = Generate.settlement()

        sql = "INSERT INTO students (education_id, full_name, birthdate, sex, place_of_residence, class) VALUES (%s, %s, %s, %s, %s, %s)"
        c.execute(
            sql,
            (education_id, full_name, birthdate, sex, place_of_residence, class_name),
        )
        con.commit()

        for n_profession in range(random.randint(0, 2)):
            profession = (Generate.profession(),)
            c.execute("SELECT id FROM professions WHERE profession = %s", profession)
            row = c.fetchone()

            if row is None:
                c.execute(
                    "INSERT INTO professions (profession) VALUES (%s)", profession
                )
                con.commit()
                row = c.lastrowid
            else:
                (row,) = row

            profession_id = row
            c.execute(
                "SELECT COUNT(1) FROM students_professions WHERE student_id = %s AND profession_id = %s",
                (education_id, profession_id),
            )
            (row,) = c.fetchone()

            if row == 0:
                c.execute(
                    "INSERT INTO students_professions (student_id, profession_id) VALUES (%s, %s)",
                    (education_id, profession_id),
                )
                con.commit()

        sql = "INSERT INTO grades (grade, subject_id, student_id) VALUES (%s, %s, %s)"
        for n_grade in range(random.randint(0, 5)):
            c.execute(
                "SELECT id FROM subjects WHERE school_subject = %s",
                (Generate.subject()["name"],),
            )
            (subject_id,) = c.fetchone()
            c.execute(sql, (Generate.grade(), subject_id, education_id))
            con.commit()

        sql = "INSERT INTO absences (absence_date, proven, class, subject_id, student_id) VALUES (%s, %s, %s, %s, %s)"
        #used_absences_ids = set()
        for n_absence in range(random.randint(0, 3)):
            absence_date = Generate.absence_date()
            proven = Generate.proven()
            classes = set([Generate.school_class() for n_class in range(1, 4)])

            c.execute(
                "SELECT id FROM subjects WHERE school_subject = %s",
                (Generate.subject()["name"],),
            )
            (subject_id,) = c.fetchone()

            for n_class in classes:
                c.execute(
                    sql, (absence_date, proven, n_class, subject_id, education_id)
                )
                con.commit()

    sql = "INSERT INTO kahoot_tests (number_of_questions, started_at, subject_id, topic_id) VALUES (%s, %s, %s, %s)"
    for n_kahoot_tests in range(KAHOOT_TESTS_MAX):
        number_of_questions = Generate.number_of_questions()
        started_at = Generate.started_at()

        c.execute(
            "SELECT id FROM subjects WHERE school_subject = %s",
            (Generate.subject()["name"],),
        )
        (subject_id,) = c.fetchone()

        c.execute(
            "SELECT id FROM topics WHERE subject_id = %s ORDER BY RAND() LIMIT 1",
            (subject_id,),
        )
        (topic_id,) = c.fetchone()

        c.execute(sql, (number_of_questions, started_at, subject_id, topic_id))
        con.commit()

    sql = "INSERT INTO kahoot_test_results (score, number_of_failed_responses, place, student_id, kahoot_test_id) VALUES (%s, %s, %s, %s, %s)"
    used_kahoot_test_ids = set()
    for n_kahoot_tests in range(KAHOOT_TESTS_MAX):
        kahoot_test_id = random.randint(1, KAHOOT_TESTS_MAX)

        while kahoot_test_id in used_kahoot_test_ids:
            kahoot_test_id = random.randint(1, KAHOOT_TESTS_MAX)

        used_kahoot_test_ids.add(kahoot_test_id)

        limit = math.ceil(STUDENTS_MAX * 0.1)

        c.execute("SELECT class FROM students ORDER BY RAND() LIMIT 1")
        (class_name,) = c.fetchone()

        c.execute(
            "SELECT education_id FROM students WHERE class = %s AND education_id NOT IN (SELECT DISTINCT student_id FROM absences INNER JOIN kahoot_tests kt ON DATE(kt.started_at) = absence_date WHERE kt.id = %s) ORDER BY RAND() LIMIT %s",
            (class_name, kahoot_test_id, limit),
        )
        students = c.fetchall()

        c.execute(
            "SELECT number_of_questions FROM kahoot_tests WHERE id = %s",
            (kahoot_test_id,),
        )
        (number_of_questions,) = c.fetchone()

        kahoot_test_results = []
        for student in students:
            number_of_failed_responses = Generate.number_of_failed_responses(
                number_of_questions
            )
            score = Generate.score(number_of_questions - number_of_failed_responses)
            (student_id,) = student

            kahoot_test_results.append(
                {
                    "score": score,
                    "number_of_failed_responses": number_of_failed_responses,
                    "student_id": student_id,
                    "kahoot_test_id": kahoot_test_id,
                }
            )

        kahoot_test_results.sort(key=lambda student: student["score"], reverse=True)

        for idx, kahoot_test_result in enumerate(kahoot_test_results):
            c.execute(
                sql,
                (
                    kahoot_test_result["score"],
                    kahoot_test_result["number_of_failed_responses"],
                    idx + 1,
                    kahoot_test_result["student_id"],
                    kahoot_test_result["kahoot_test_id"],
                ),
            )
            con.commit()


if __name__ == "__main__":
    main()
