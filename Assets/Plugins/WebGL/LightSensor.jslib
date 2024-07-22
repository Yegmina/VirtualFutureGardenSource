mergeInto(LibraryManager.library, {
    StartLightSensor: function () {
        if ('AmbientLightSensor' in window) {
            try {
                console.log("AmbientLightSensor supported, initializing...");
                const sensor = new AmbientLightSensor({ frequency: 1 });

                sensor.addEventListener('reading', function() {
                    var lightValue = sensor.illuminance;
                    console.log('AmbientLightSensor reading:', lightValue);
                    if (lightValue === 0) {
                        console.warn('Received light value is 0, which is unexpected.');
                    }
                    console.log('Sending message to Unity:', lightValue.toString());
                    SendMessage('LightSensorObject', 'OnLightSensorChanged', lightValue.toString());
                });

                sensor.addEventListener('error', function(event) {
                    console.error('Sensor error:', event.error.name, event.error.message);
                });

                sensor.start();
                console.log("AmbientLightSensor started.");
            } catch (error) {
                console.error('Failed to start sensor:', error);
            }
        } else {
            console.warn("AmbientLightSensor is not supported by this browser.");
        }
    }
});
