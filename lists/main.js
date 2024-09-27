const list1 = new List('group1', {
    list: document.getElementById('list1'),
    lengthDisplay: document.getElementById('list1-length'),
}, {
    moveAll: document.getElementById('list1Tolist2All'),
    moveSelected: document.getElementById('list1Tolist2'),
    reverseList: document.getElementById('reverse-list'),
});

const list2 = new List('group1', {
    list: document.getElementById('list2'),
    lengthDisplay: document.getElementById('list2-length'),
}, {
    moveAll: document.getElementById('list2Tolist1All'),
    moveSelected: document.getElementById('list2Tolist1'),
    reverseList: document.getElementById('reverse-list'),
});
