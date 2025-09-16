import { useState, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { TaskActionMode, TaskModel } from "../../types";
import { useStyles } from "./style";
import { TaskForm, TasksList } from "./components";

export const Tasks = () => {
  const { i18n } = useTranslation();
  const classes = useStyles();
  const dir = i18n.dir();
  const isRtl = dir === "rtl";

  const [selectedTask, setSelectedTask] = useState<TaskModel | null>(null);
  const [actionMode, setActionMode] = useState<TaskActionMode>('create');
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const handleTaskSelect = useCallback((task: TaskModel, mode: TaskActionMode) => {
    setSelectedTask(task);
    setActionMode(mode);
  }, []);

  const handleTaskDeselect = useCallback(() => {
    setSelectedTask(null);
    setActionMode('create');
  }, []);

  const handleTaskCreated = useCallback(() => {
    setSelectedTask(null);
    setActionMode('create');
  }, []);

  const handleTaskUpdated = useCallback(() => {
    setSelectedTask(null);
    setActionMode('create');
  }, []);

  const handleCreateNew = useCallback(() => {
    setSelectedTask(null);
    setActionMode('create');
  }, []);

  const handleRefreshTaskList = useCallback(() => {
    setRefreshTrigger(prev => prev + 1);
  }, []);

  return (
    <div className={classes.root} data-testid="tasks-page">
      <TaskForm
        isRtl={isRtl}
        selectedTask={selectedTask}
        actionMode={actionMode}
        onTaskCreated={handleTaskCreated}
        onTaskUpdated={handleTaskUpdated}
        onCancel={handleTaskDeselect}
        onRefreshTaskList={handleRefreshTaskList}
        onCreateNew={handleCreateNew}
      />
      <TasksList
        dir={dir}
        onTaskSelect={handleTaskSelect}
        refreshTrigger={refreshTrigger}
      />
    </div>
  );
};