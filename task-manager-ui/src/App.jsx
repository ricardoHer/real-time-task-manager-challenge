// import { useState } from 'react'
import { useEffect, useState } from "react";
import "./App.css";
import axios from "axios";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

import TaskList from './components/TaskList'
import TaskForm from "./components/TaskForm";
import TaskSummary from "./components/TaskSummary";

const API_BASE_URL = "http://localhost:5261";

function App() {
  const [tasks, setTasks] = useState([]);
  const [connection, setConnection] = useState(null);
  const [summary, setSummary] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // fething initial tasks
    fetchTasks();

    // Setup the SignalR connectio
    const newConnection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/taskhub`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          console.log("Connected to SignalR hub");
          // listen for new Tasks
          connection.on("TaskAdded", (newTask) => {
            setTasks((prevTasks) => [newTask, ...prevTasks]);
          });
        })
        .catch((err) => console.error("Connection failed: ", err));
    }

    return () => {
      if (connection) {
        connection.stop();
      }
    };
  }, [connection]);

  const fetchTasks = async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/tasks`);
      setTasks(response.data);
    } catch (error) {
      console.error("Error fetching tasks: ", error);
    }
  };

  const addTask = async (taskData) => {
    try {
      await axios.post(`${API_BASE_URL}/api/tasks`, taskData);
    } catch (error) {
      console.error("Error adding new task: ", error);
    }
  };

  const generateSummary = async () => {
    setLoading(true);
    try {
      const response = await axios.post(`${API_BASE_URL}/api/tasks/summary`);
      setSummary(response.data.summary);
    } catch (error) {
      console.error("Error getting the summary", error);
      setSummary("Error on generating the summary, try again later");
    }
  };

  return (
    <div className="app">
      <header className="app-header">
        <h1>Real-Time Task Manager</h1>
        <p>Add tasks and see instant updates!</p>
      </header>

      <main className="app-main">
        <div className="app-grid">
          <div className="task-section">
            <TaskForm onSubmit={addTask} />
            <TaskList tasks={tasks} />
          </div>

          <div className="summary-section">
            <TaskSummary
              summary={summary}
              onGenerate={generateSummary}
              loading={loading}
              taskCount={tasks.length}
            />
          </div>
        </div>
      </main>
    </div>
  );
}

export default App;
