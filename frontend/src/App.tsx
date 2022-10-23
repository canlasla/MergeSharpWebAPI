import React from 'react';
import logo from './logo.svg';
import './App.css';
import { useState, useEffect } from 'react';

type resultProps = {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}


function App() {
    const [result, setResult] = useState<resultProps[]>([]);

    useEffect(() => {
        const api = async () => {
            const data = await fetch("https://localhost:7009/weatherforecast", {
                method: "GET"
            });
            const jsonData = await data.json();
            setResult(jsonData.results);
        };

        api();
    }, []);
    return (
        <div className="App">
            <h1>
                {result.map((value) => {
                    return (
                        <div>
                            <div>{value.date}</div>
                            <div>{value.temperatureC}</div>
                            <div>{value.temperatureF}</div>
                            <div>{value.summary}</div>
                        </div>
                    );
                })}
            </h1>
        </div>
    );
}

export default App;
