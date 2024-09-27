/**
 * List control buttons
 * @typedef {{
 *     moveAll: HTMLElement,
 *     moveSelected: HTMLElement,
 *     reverseList: HTMLInputElement
 * }} Controls
 */


/**
 * Display and list elements
 * @typedef {{
*     lengthDisplay: HTMLElement,
*     list: HTMLOListElement | HTMLUListElement,
* }} Elements
*/


class List {
    #group;
    #lengthDisplay;
    #list;
    #controls;
    #symbol;
    #event;

    /**
     * @param {string} group - Group name
     * @param {Elements} elements
     * @param {Controls} controls - Control elements
     */
    constructor(group, elements, controls) {
        this.#group = group;
        this.#lengthDisplay = elements.lengthDisplay;
        this.#list = elements.list;
        this.#controls = controls;
        this.#symbol = Symbol();
        this.#event = new CustomEvent(this.#group, { detail: {} });
        this.#updateDisplay();
        this.#sortList();
        this.#disableControls();

        this.#controls.reverseList.addEventListener('change', () => this.#sortList());

        this.#list.addEventListener('click', (event) => this.#select(event));
        this.#list.addEventListener('dblclick', (event) => {
            this.#select(event);
            this.#moveSelected.bind(this)();
        });

        document.addEventListener(this.#group, this.#listener.bind(this));

        this.#controls.moveAll.addEventListener('click', this.#moveAll.bind(this));
        this.#controls.moveSelected.addEventListener('click', this.#moveSelected.bind(this));
    }

    /**
     * @param {Event} event
     */
    #select(event) {
        /** @type {HTMLElement} */
        const target = event.target;

        if (target.classList.contains('selected')) {
            target.classList.remove('selected');
        } else if (target.tagName !== 'OL' && target.tagName !== 'UL') {
            target.classList.add('selected');
        }

        this.#disableControls();
    }

    /**
     * @param {ListEvent} event
     */
    #listener(event) {
        if (event.detail.from !== this.#symbol) {
            this.#list.append(...event.detail.data);
            this.#updateDisplay();
            this.#sortList();
            this.#disableControls();
        }
    }

    /**
     * @param {HTMLElement[]} elements
     */
    #clearSelect(elements) {
        for (const element of elements) {
            element.classList.remove('selected');
        }
    }

    #moveAll() {
        this.#event.detail.from = this.#symbol;
        this.#event.detail.data = this.#list.children;
        this.#clearSelect(this.#event.detail.data);
        document.dispatchEvent(this.#event);
        this.#updateDisplay();
        this.#disableControls();
    }

    #moveSelected() {
        this.#event.detail.from = this.#symbol;
        this.#event.detail.data = this.#list.querySelectorAll('.selected');
        this.#clearSelect(this.#event.detail.data);
        document.dispatchEvent(this.#event);
        this.#updateDisplay();
        this.#disableControls();
    }

    #updateDisplay() {
        this.#lengthDisplay.innerText = this.#list.children.length;
    }

    #sortList() {
        if (this.#controls.reverseList.checked) {
            this.#list.append(...Array.from(this.#list.children)
                .sort((a, b) => b.innerText.localeCompare(a.innerText)));
        } else {
            this.#list.append(...Array.from(this.#list.children)
                .sort((a, b) => a.innerText.localeCompare(b.innerText)));
        }
    }

    #disableControls() {
        if (this.#list.children.length === 0) {
            this.#controls.moveAll.disabled = true;
            this.#controls.moveSelected.disabled = true;
        } else {
            this.#controls.moveAll.disabled = false;
            this.#controls.moveSelected.disabled = false;
        }

        if (this.#list.querySelectorAll('.selected').length === 0) {
            this.#controls.moveSelected.disabled = true;
        } else {
            this.#controls.moveSelected.disabled = false;
        }
    }
}
