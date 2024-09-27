const { createApp, ref, onMounted, watch } = Vue;
const { DateTime } = luxon;

const app = createApp({
    setup() { },
});

const calendar = {
    props: {
        year: Number,
        month: Number,
        day: Number,
        title: String,
    },
    setup(props) {
        /**
         * date format: LL-dd
         */
        const specialDays = [
            { date: '01-01', text: 'Újév', },
            { date: '03-15', text: 'Nemzeti ünnep', },
            { date: '05-01', text: 'Munka ünnepe', },
            { date: '08-20', text: 'Nemzeti ünnep', },
            { date: '10-23', text: 'Nemzeti ünnep', },
            { date: '11-01', text: 'Mindenszentek', },
            { date: '12-24', text: 'Szenteste', },
            { date: '12-25', text: 'Karácsony', },
            { date: '12-26', text: 'Karácsony', },
            // TODO: https://unnepnapok.com/munkaszuneti-napok-unnepek-2024-magyarorszag/
        ];
        const currentTime = ref(getCurrentTime(props.year, props.month, props.day));
        const today = currentTime.value;
        const weekdayShorts = ref([]);
        const weeks = ref([]);
        let currentPanelDateTimes = getCurrentPanelDateTimes();

        weekdayShorts.value = getWeekdayShorts(currentPanelDateTimes);
        weeks.value = getWeeks(currentPanelDateTimes);

        onMounted(() => {
            if (props.title) document.title = `${props.title} ${currentTime.value.year}`;
        });

        watch(currentTime, async () => {
            if (props.title) document.title = `${props.title} ${currentTime.value.year}`;
            currentPanelDateTimes = getCurrentPanelDateTimes();
            weekdayShorts.value = getWeekdayShorts(currentPanelDateTimes);
            weeks.value = getWeeks(currentPanelDateTimes);
        });

        function getCurrentTime(year, month, day) {
            let currentTime = DateTime.now().setLocale('hu');

            return currentTime.set({
                year: year ?? currentTime.year,
                month: month ?? currentTime.month,
                day: day ?? currentTime.day,
            });
        }

        /**
         * Calculates out the DateTime values in the current month
         *
         * @returns DateTime values in the current month
         */
        function getCurrentPanelDateTimes() {
            let time = currentTime.value.startOf('month').startOf('week');

            if (currentTime.value.startOf('month').weekday === 1) {
                time = time.minus({ days: 7 });
            }

            const rows = 6;
            const days = 7;

            const dateTimes = [];
            for (let day = 0; day < rows * days; day++) {
                dateTimes.push(time);
                time = time.plus({ day: 1 });
            }

            return dateTimes;
        }

        /**
         *
         * @returns Returns the name of the days
         */
        function getWeekdayShorts(currentPanelDateTimes) {
            return currentPanelDateTimes.slice(0, 7).map((time) => time.weekdayShort);
        }

        /**
         *
         * @returns Returns the days of weeks in seven element chunks
         */
        function getWeeks(currentPanelDateTimes) {
            function getSpecialDay(time) {
                const specialDay = specialDays.find((day) => day.date === time.toFormat('LL-dd'));

                if (typeof specialDay !== 'undefined') {
                    return {
                        isSpecialDay: true,
                        text: specialDay.text,
                    };
                } else {
                    return {
                        isSpecialDay: false,
                    };
                }
            }

            const chunkSize = 7;
            const weeks = [];

            for (let sliceStart = 0; sliceStart < currentPanelDateTimes.length; sliceStart += chunkSize) {
                const chunk = currentPanelDateTimes.slice(sliceStart, sliceStart + chunkSize).map((time) => {
                    return {
                        isToday: time.toISODate() === today.toISODate(),
                        isInCurrentMonth: time.toFormat('yyyy-LL') === currentTime.value.toFormat('yyyy-LL'),
                        isSaturday: time.weekday === 6,
                        isSunday: time.weekday === 7,
                        specialDay: getSpecialDay(time),
                        value: time.toFormat('dd'),
                    };
                });
                weeks.push(chunk);
            }

            return weeks;
        }

        return {
            weekdays: weekdayShorts,
            weeks,
            currentTime,
        };
    },
    methods: {
        changeYear(number) {
            this.currentTime = this.currentTime.plus({ years: number });
        },
        changeMonth(number) {
            this.currentTime = this.currentTime.plus({ months: number });
        }
    },
    template: '#calendar',
}

app.component('calendar', calendar);

app.mount('#app');
