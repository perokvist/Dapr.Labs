const express = require('express');
const bodyParser = require('body-parser');

const app = express();
// Dapr publishes messages with the application/cloudevents+json content-type
app.use(bodyParser.json({ type: 'application/*+json' }));

const port = 3000;

app.get('/dapr/subscribe', (_req, res) => {
    res.json([
        'sample'
    ]);
});

app.post('/sample', (req, res) => {
    console.log("JavaScript got event ", req.body);
    res.sendStatus(200);
});

app.listen(port, () => console.log(`Node App listening on port ${port}!`));