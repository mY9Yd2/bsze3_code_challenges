-- Kinek van csak 5-ös osztályzata az osztályból?
SELECT s.full_name
FROM grades g
INNER JOIN students s
    ON s.education_id = g.student_id
WHERE s.class = '1/13.be szoft'
GROUP BY g.student_id
HAVING MIN(g.grade) = 5;


-- Ki hiányzott a legtöbbet az osztályból igazoltan?
SELECT s.full_name
FROM absences a
INNER JOIN students s
	ON s.education_id = a.student_id
WHERE a.proven IS TRUE AND s.class = '1/13.be szoft'
GROUP BY s.education_id
HAVING COUNT(1) = (
    -- Mi a legnagyobb érték
    SELECT COUNT(1) AS c
    FROM absences a2
    INNER JOIN students s2
		ON s2.education_id = a2.student_id
    WHERE proven IS TRUE AND s2.class = '1/13.be szoft'
    GROUP BY a2.student_id
    ORDER BY c DESC
    LIMIT 1
);


-- Melyik az a három tanuló, aki legtöbbet hiányzott igazolatlanul?
SELECT s.education_id, s.full_name, COUNT(1) AS absence
FROM absences a
INNER JOIN students s
	ON s.education_id = a.student_id
WHERE a.proven IS FALSE
GROUP BY s.education_id
ORDER BY absence DESC
LIMIT 3; -- Miért három kell? Mindegy.


-- Kahoot teszten ki töltötte ki a legtöbb tesztet?
SELECT s.education_id, s.full_name
FROM kahoot_tests kt
INNER JOIN kahoot_test_results ktr
	ON ktr.kahoot_test_id = kt.id
INNER JOIN students s
	ON s.education_id = ktr.student_id
INNER JOIN subjects s2
	ON
		s2.school_subject LIKE '%C#%'
		AND s2.id = kt.subject_id
GROUP BY s.education_id
HAVING COUNT(1) = (
    -- Mi a legnagyobb érték
	SELECT COUNT(1) AS c
	FROM kahoot_tests kt
	INNER JOIN kahoot_test_results ktr
		ON ktr.kahoot_test_id = kt.id
	INNER JOIN students s
		ON s.education_id = ktr.student_id
	INNER JOIN subjects s2
		ON
			s2.school_subject LIKE '%C#%'
			AND s2.id = kt.subject_id
	GROUP BY s.education_id
	ORDER BY c DESC
	LIMIT 1
);


-- Kahoot teszteken elért összpontszám alapján irasd ki a legjobb 5 tanulót fordított sorrendben (5. helyezettől indítva!)
-- (mezők, amiknek szerepelnie kell az eredményben:
-- helyezés, játékos neve, összpontszám, hány tesztből érte el, hányat hibázott  az összes kérdésből)
SELECT
	ROW_NUMBER() OVER(ORDER BY sum_score DESC) AS place,
	q.*
FROM (
	SELECT DISTINCT
		s.full_name,
		SUM(ktr.score) OVER(PARTITION BY ktr.student_id) AS sum_score,
		COUNT(1) OVER(PARTITION BY ktr.student_id) AS count_number_of_tests,
		SUM(ktr.number_of_failed_responses) OVER(PARTITION BY ktr.student_id) AS sum_number_of_failed_responses
	FROM students s
	INNER JOIN kahoot_test_results ktr
		ON ktr.student_id = s.education_id
	INNER JOIN kahoot_tests kt
		ON kt.id = ktr.kahoot_test_id
	INNER JOIN subjects s2
		ON
			s2.school_subject LIKE '%C#%'
			AND s2.id = kt.subject_id
	ORDER BY sum_score DESC
	LIMIT 5
) AS q
ORDER BY q.sum_score;


-- Kinek sikerült olyan tesztet írnia, aminél nem hibázott egyetlen kérdést sem, hány ilyen volt neki?
SELECT s.education_id, s.full_name, COUNT(1)
FROM kahoot_tests kt
INNER JOIN kahoot_test_results ktr
	ON ktr.kahoot_test_id = kt.id
INNER JOIN students s
	ON s.education_id = ktr.student_id
INNER JOIN subjects s2
	ON
		s2.school_subject LIKE '%C#%'
		AND s2.id = kt.subject_id
WHERE ktr.number_of_failed_responses = 0
GROUP BY s.education_id;


-- Melyik férfi és nő érte el a legalacsonyabb pontszámot a kahoot teszteken?
WITH min_scores AS (
	SELECT DISTINCT
		s.full_name,
		s.sex,
		MIN(ktr.score) OVER(PARTITION BY ktr.student_id) AS min_score
	FROM students s
	INNER JOIN kahoot_test_results ktr
		ON ktr.student_id = s.education_id
	INNER JOIN kahoot_tests kt
		ON kt.id = ktr.kahoot_test_id
	INNER JOIN subjects s2
		ON
			s2.school_subject LIKE '%C#%'
			AND s2.id = kt.subject_id
)
(SELECT ms.full_name, ms.sex
	FROM min_scores ms
	WHERE
		ms.sex = 'Male'
		AND ms.min_score = (
			SELECT MIN(min_score)
			FROM min_scores
			WHERE min_scores.sex = 'Male'
		)
) UNION
(SELECT ms.full_name, ms.sex
	FROM min_scores ms
	WHERE
		ms.sex = 'Female'
		AND ms.min_score = (
			SELECT MIN(min_score)
			FROM min_scores
			WHERE min_scores.sex = 'Female'
		)
);


-- Ki a legfiatalabb, akinek sikerült már hibátlan kahoot tesztet kitöltenie?
SELECT q.education_id, q.full_name
FROM (
	SELECT
        s.education_id,
        s.full_name,
        s.age,
        MIN(s.age) OVER() AS min_age
	FROM kahoot_tests kt
	INNER JOIN kahoot_test_results ktr
		ON ktr.kahoot_test_id = kt.id
	INNER JOIN students s
		ON s.education_id = ktr.student_id
	INNER JOIN subjects s2
		ON
			s2.school_subject LIKE '%C#%'
			AND s2.id = kt.subject_id
	WHERE ktr.number_of_failed_responses = 0
	GROUP BY s.education_id
) AS q
WHERE q.age = q.min_age;
