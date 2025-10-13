import TaskItem from "./TaskItem";

function TaskList({ tasks }) {
  if (tasks.length == 0) {
    return (
      <div className="task-list-container">
        <h2> Task list</h2>
        <div className="empty-state">
          <p>No tasks found</p>
          <p>Add your first task</p>
        </div>
      </div>
    );
  }


  return (
      <div className="task-list-container">
      <h2>Task List ({tasks.length})</h2>
      <div className="task-list">
        {tasks.map((task) => (
          <TaskItem key={task.id} task={task} />
        ))}
      </div>
    </div>
  )
}


export default TaskList