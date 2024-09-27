import random

from data import Data


class Generate:

    @staticmethod
    def education_id() -> str:
        return str(random.randint(0, 99999999999)).rjust(11, "0")

    @staticmethod
    def full_name(sex: str = "Male") -> str:
        forename = (
            random.choice(Data.male_forenames)
            if sex.capitalize() == "Male"
            else random.choice(Data.female_forenames)
        )
        return f"{random.choice(Data.surnames)} {forename}"

    @staticmethod
    def birthdate() -> str:
        year = str(random.randint(1945, 2004))
        month = str(random.randint(1, 12)).rjust(2, "0")
        # Max. 28 to keep it simple
        day = str(random.randint(1, 28)).rjust(2, "0")
        return f"{year}-{month}-{day}"

    @staticmethod
    def sex() -> str:
        return ("Male", "Female")[random.randint(0, 1)]

    @staticmethod
    def settlement() -> str:
        return random.choice(Data.settlements)

    @staticmethod
    def profession() -> str:
        return random.choice(Data.professions)

    @staticmethod
    def subject() -> dict[str, str | list[str]]:
        return random.choice(Data.subjects)

    @staticmethod
    def grade() -> int:
        return random.randint(1, 5)

    @staticmethod
    def absence_date() -> str:
        year = "2024"
        month = str(random.randint(1, 12)).rjust(2, "0")
        # Max. 28 to keep it simple
        day = str(random.randint(1, 28)).rjust(2, "0")
        return f"{year}-{month}-{day}"

    @staticmethod
    def started_at() -> str:
        hour = str(random.randint(7, 20)).rjust(2, "0")
        minute = random.choice(["45", "30", "15", "00"])
        return f"{Generate.absence_date()} {hour}:{minute}:00"

    @staticmethod
    def proven() -> bool:
        return random.choices((False, True), weights=[50, 1], k=1)[0]

    @staticmethod
    def school_class() -> int:
        return random.randint(0, 15)

    @staticmethod
    def number_of_questions() -> int:
        return random.randint(
            1, random.choices((64, 128, 255), weights=[20, 2, 1], k=1)[0]
        )

    @staticmethod
    def number_of_failed_responses(number_of_questions: int) -> int:
        return random.randint(0, number_of_questions)

    @staticmethod
    def score(number_of_successful_responses) -> int:
        return sum(
            [
                random.randint(600, 950)
                for response in range(number_of_successful_responses)
            ]
        )
