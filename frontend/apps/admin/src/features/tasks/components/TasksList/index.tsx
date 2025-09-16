import { useMemo, useState, useEffect } from "react";
import {
  IconButton,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Chip,
} from "@mui/material";
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
} from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";
import { useQueryClient } from "@tanstack/react-query";
import { useGetAllTasks, useDeleteTask, taskKeys } from "../../../../api";
import { TaskActionMode, TaskModel } from "../../../../types";
import { useStyles } from "./style";

interface TasksListProps {
  dir: "ltr" | "rtl";
  onTaskSelect: (task: TaskModel, mode: TaskActionMode) => void;
  refreshTrigger: number;
}

export const TasksList = ({ dir, onTaskSelect, refreshTrigger }: TasksListProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const queryClient = useQueryClient();

  const {
    data: tasks,
    isLoading: isTasksLoading,
    error: getTasksError,
  } = useGetAllTasks();

  const { mutate: deleteTask, isPending: isDeletingTask } = useDeleteTask();

  // Listen for refresh trigger changes and invalidate queries
  useEffect(() => {
    if (refreshTrigger > 0) {
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
    }
  }, [refreshTrigger, queryClient]);

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deletingTaskId, setDeletingTaskId] = useState<number | null>(null);

  const filteredTasks = useMemo(() => {
    if (!tasks) return [];
    const q = search.trim().toLowerCase();
    if (!q) return tasks;
    return tasks.filter((task) =>
      [task.id.toString(), task.name, task.payload].some((field) =>
        field.toLowerCase().includes(q)
      )
    );
  }, [tasks, search]);

  const currentPageTasks = useMemo(() => {
    if (rowsPerPage <= 0) return filteredTasks;
    const start = page * rowsPerPage;
    return filteredTasks.slice(start, start + rowsPerPage);
  }, [filteredTasks, page, rowsPerPage]);

  const handlePageChange = (_: unknown, newPage: number) => setPage(newPage);

  const handleRowsPerPageChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleDeleteTask = (task: TaskModel) => {
    if (window.confirm(t("pages.tasks.confirmDelete", { name: task.name }))) {
      setDeletingTaskId(task.id);
      deleteTask(task.id, {
        onSuccess: () => {
          toast.success(t("pages.tasks.taskDeleted"));
        },
        onError: (error: Error) => {
          toast.error(error.message || t("pages.tasks.taskDeleteFailed"));
        },
        onSettled: () => {
          setDeletingTaskId(null);
        },
      });
    }
  };

  const truncateText = (text: string, maxLength: number = 50) => {
    return text.length > maxLength ? `${text.substring(0, maxLength)}...` : text;
  };

  return (
    <div className={classes.listContainer} data-testid="tasks-list">
      <h2 className={classes.sectionTitle}>{t("pages.tasks.tasks")}</h2>

      {isTasksLoading && <p>{t("pages.tasks.loadingTasks")}</p>}
      {getTasksError && (
        <p style={{ color: "#c00" }}>{t("pages.tasks.loadTasksFailed")}</p>
      )}

      {!isTasksLoading && !getTasksError && (
        <div className={classes.tableArea} data-testid="tasks-table">
          <div className={classes.tableShell} data-testid="tasks-table-shell">
            <Table
              size="small"
              className={classes.headerTable}
              aria-label="tasks header"
            >
              <TableHead>
                <TableRow>
                  <TableCell align="center" width="10%">
                    {t("pages.tasks.taskId")}
                  </TableCell>
                  <TableCell align="center" width="25%">
                    {t("pages.tasks.taskName")}
                  </TableCell>
                  <TableCell align="center" width="40%">
                    {t("pages.tasks.taskPayload")}
                  </TableCell>
                  <TableCell align="center" width="25%">
                    {t("pages.tasks.actions")}
                  </TableCell>
                </TableRow>
              </TableHead>
            </Table>

            <div className={classes.searchBar} data-testid="tasks-search-bar">
              <TextField
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder={t("pages.tasks.searchPlaceholder")}
                size="small"
                fullWidth
                slotProps={{
                  htmlInput: { "data-testid": "tasks-search-input" },
                  input: {
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon fontSize="small" />
                      </InputAdornment>
                    ),
                    endAdornment: search ? (
                      <InputAdornment position="end">
                        <IconButton
                          size="small"
                          aria-label={t("pages.tasks.clear")}
                          onClick={() => setSearch("")}
                          data-testid="tasks-search-clear"
                        >
                          <ClearIcon fontSize="small" />
                        </IconButton>
                      </InputAdornment>
                    ) : null,
                  },
                }}
                className={classes.searchField}
                dir={dir}
              />
            </div>

            <div className={classes.rowsScroll} data-testid="tasks-rows-scroll">
              <Table
                size="small"
                className={classes.bodyTable}
                aria-label="tasks body"
              >
                <TableBody>
                  {currentPageTasks.map((task) => (
                    <TableRow
                      key={task.id}
                      className={classes.tableRow}
                      data-testid={`task-row-${task.id}`}
                    >
                      <TableCell align="center">
                        <Chip
                          label={task.id}
                          size="small"
                          variant="outlined"
                          color="primary"
                        />
                      </TableCell>
                      <TableCell align="left">
                        <div className={classes.taskName}>
                          {truncateText(task.name, 30)}
                        </div>
                      </TableCell>
                      <TableCell align="left">
                        <div className={classes.taskPayload}>
                          {truncateText(task.payload, 60)}
                        </div>
                      </TableCell>
                      <TableCell align="center">
                        <div className={classes.actionsContainer}>
                          <IconButton
                            size="small"
                            onClick={() => onTaskSelect(task, 'view')}
                            title={t("pages.tasks.viewTask")}
                            data-testid={`task-view-${task.id}`}
                          >
                            <ViewIcon fontSize="small" />
                          </IconButton>
                          <IconButton
                            size="small"
                            onClick={() => onTaskSelect(task, 'edit')}
                            title={t("pages.tasks.editTask")}
                            data-testid={`task-edit-${task.id}`}
                          >
                            <EditIcon fontSize="small" />
                          </IconButton>
                          <IconButton
                            size="small"
                            onClick={() => handleDeleteTask(task)}
                            disabled={isDeletingTask && deletingTaskId === task.id}
                            title={t("pages.tasks.deleteTask")}
                            color="error"
                            data-testid={`task-delete-${task.id}`}
                          >
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                  {filteredTasks.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={4} align="center">
                        {search 
                          ? t("pages.tasks.noTasksFound") 
                          : t("pages.tasks.noTasks")
                        }
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </div>
          </div>

          <TablePagination
            component="div"
            className={classes.paginationBar}
            data-testid="tasks-pagination"
            count={filteredTasks.length}
            page={page}
            onPageChange={handlePageChange}
            rowsPerPage={rowsPerPage}
            onRowsPerPageChange={handleRowsPerPageChange}
            rowsPerPageOptions={[
              5,
              10,
              25,
              { label: t("pages.tasks.all"), value: -1 },
            ]}
            labelRowsPerPage={t("pages.tasks.rowsPerPage")}
            labelDisplayedRows={({ from, to, count }) =>
              `${from}-${to} ${t("pages.tasks.of")} ${count !== -1 ? count : to}`
            }
          />
        </div>
      )}
    </div>
  );
};