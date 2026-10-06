window.requestNotificationPermission = async () => {
    const permission = await Notification.requestPermission();
    if (permission === "granted") {
        console.log("Notification permission granted.");
    } else {
        console.log("Notification permission denied.");
    }
};

window.sendNotification = (title, options) => {
    if (Notification.permission === "granted") {
        new Notification(title, options);
    }
};
