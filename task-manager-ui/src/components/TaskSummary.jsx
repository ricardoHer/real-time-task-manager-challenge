function TaskSummary({ summary, onGenerate, loading, taskCount }) {
  return (
    <div className="summary-section">
      <h2>AI Summary</h2>

      <button
        onClick={onGenerate}
        disabled={loading || taskCount === 0}
        className="generate-btn"
      >
        {loading ? "Generating..." : "Generate Summary"}
      </button>

      {taskCount === 0 && (
        <p className="loading-text">
          Add tasks to generate a summary
        </p>
      )}

      {summary && (
        <div className="summary-content">
          {summary}
        </div>
      )}
    </div>
  );
}

export default TaskSummary;
