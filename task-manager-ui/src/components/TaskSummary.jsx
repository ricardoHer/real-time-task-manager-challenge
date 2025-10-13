function TaskSummary({ summary, onGenerate, loading, taskCount }) {
  return (
    <div className="task-summary-container">
      <h2>🤖 Resumo das Tarefas</h2>

      <button
        onClick={onGenerate}
        disabled={loading || taskCount === 0}
        className="summary-btn"
      >
        {loading ? "Gerando resumo..." : "Gerar Resumo com IA"}
      </button>

      {taskCount === 0 && (
        <p className="summary-empty">
          Adicione algumas tarefas para gerar um resumo!
        </p>
      )}

      {summary && (
        <div className="summary-content">
          <h3>📝 Resumo:</h3>
          <div className="summary-text">
            {summary.split("\n").map((line, index) => (
              <p key={index}>{line}</p>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

export default TaskSummary;
