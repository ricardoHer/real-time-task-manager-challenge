// import { useState } from 'react'
import { useEffect, useState } from "react";
import "./App.css";
import axios from "axios";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const API_BASE_URL = "http://localhost:5261";

function App() {
  const [tasks, setTasks] = useState([]);
  const [connection, setCoinnection] = useState(null);

  useEffect(() => {
    // fething initial tasks
    fetchTasks();

    // Setup the SignalR connectio
    const newConnection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/taskhub`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    setCoinnection(newConnection);
  }, []);

  const fetchTasks = async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/tasks`);
      setTasks(response.data);
    } catch (error) {
      console.error("Error fetching tasks: ", error);
    }
  };

  return (
    <>
      <div></div>
    </>
  );
}

export default App;
