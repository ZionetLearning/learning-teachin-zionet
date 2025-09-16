import { useEffect, useMemo, useCallback } from "react";
import { ErrorMessage, Field, Form, Formik } from "formik";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";
import { 
  useCreateTask,
  useUpdateTaskName,
  useGetTaskById 
} from "../../../../api";
import { useSignalR } from "../../../../hooks";
import { TaskActionMode, TaskModel } from "../../../../types";
import { CreateTaskFormValues, validationSchema } from "../../validation";
import { useStyles } from "./style";
import { UserNotification } from "@app-providers/types";

function generateRandomId() {
  return Math.floor(Math.random() * 2147483647) + 1;
}

interface TaskFormProps {
  isRtl: boolean;
  selectedTask: TaskModel | null;
  actionMode: TaskActionMode;
  onTaskCreated: () => void;
  onTaskUpdated: () => void;
  onCancel: () => void;
}

export const TaskForm = ({
  isRtl,
  selectedTask,
  actionMode,
  onTaskCreated,
  onTaskUpdated,
  onCancel,
}: TaskFormProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const { mutate: createTask, isPending: isCreatingTask } = useCreateTask();
  const { mutate: updateTaskName, isPending: isUpdating } = useUpdateTaskName();
  const { data: taskWithETag } = useGetTaskById(selectedTask?.id || 0);
  const { status: signalRStatus, subscribe } = useSignalR();

  const isEditing = actionMode === 'edit';
  const isViewing = actionMode === 'view';
  const isFormDisabled = isViewing || isCreatingTask || isUpdating;

  const handleTaskNotification = useCallback((notification: UserNotification) => {
    if (notification.type === 'Success' && notification.message.includes('Task created')) {

      toast.success(notification.message);
    } else if (notification.type === 'Success' && notification.message.includes('Task updated')) {
      toast.success(notification.message);
    } else if (notification.type === 'Error' && notification.message.includes('Task')) {

      toast.error(notification.message);
    }
  }, []);

  useEffect(() => {
    if (signalRStatus === 'connected') {
      const unsubscribe = subscribe<UserNotification>(
        "NotificationMessage",
        handleTaskNotification
      );
      return unsubscribe;
    }
  }, [signalRStatus, subscribe, handleTaskNotification]);

    const initialValues = useMemo((): CreateTaskFormValues => {
    if (selectedTask && (isEditing || isViewing)) {
      return {
        id: selectedTask.id.toString(),
        name: selectedTask.name,
        payload: selectedTask.payload,
      };
    }
    return {
      name: "",
      payload: "",
    };
  }, [selectedTask, isEditing, isViewing]);

  const handleSubmit = (values: CreateTaskFormValues, helpers: { resetForm: () => void; setSubmitting: (isSubmitting: boolean) => void }) => {
    const { resetForm, setSubmitting } = helpers;
    if (isEditing && selectedTask) {
      updateTaskName(
        {
          id: selectedTask.id,
          name: values.name.trim(),
          etag: taskWithETag?.etag,
        },
        {
          onSuccess: () => {
            toast.success(t("pages.tasks.taskUpdated"));
            onTaskUpdated();
            resetForm();
          },
          onError: (error: Error) => {
            toast.error(error.message || t("pages.tasks.taskUpdateFailed"));
          },
          onSettled: () => setSubmitting(false),
        }
      );
    } else {
          createTask(
            {
              id: generateRandomId(),
              name: values.name.trim(),
              payload: values.payload.trim(),
            },
            {
              onSuccess: () => {
                toast.success(t("pages.tasks.taskCreated"));
                onTaskCreated();
                resetForm();
              },
              onError: (error: Error) => {
                toast.error(error.message || t("pages.tasks.taskCreationFailed"));
              },
              onSettled: () => setSubmitting(false),
            }
          );
    }

  };

  return (
    <div
      className={classes.creationContainer}
      style={{ textAlign: isRtl ? "right" : "left" }}
    >
      <h2 className={classes.sectionTitle}>
        {isViewing
          ? t("pages.tasks.viewTask")
          : isEditing
          ? t("pages.tasks.editTask")
          : t("pages.tasks.createTask")}
      </h2>
      <div>
        <span className={classes.statusLabel}>SignalR: </span>
        <span
          className={`${classes.statusValue} ${
            signalRStatus === 'connected' ? classes.statusConnected : classes.statusDisconnected
          }`}
        >
          {signalRStatus}
        </span>
      </div>
      <Formik
        key={`${actionMode}-${selectedTask?.id || 'new'}`}
        initialValues={initialValues}
        validationSchema={validationSchema}
        onSubmit={handleSubmit}
        enableReinitialize
      >
        {({ isSubmitting, resetForm }) => (
          <Form>
            <label className={classes.label}>
              {t("pages.tasks.taskName")}
              <Field
                name="name"
                type="text"
                placeholder={t("pages.tasks.taskNamePlaceholder")}
                disabled={isViewing || isFormDisabled}
                data-testid="task-form-name"
              />
              <span className={classes.error}>
                <ErrorMessage name="name" />
              </span>
            </label>

            <label className={classes.label}>
              {t("pages.tasks.taskPayload")}
              <Field
                as="textarea"
                name="payload"
                placeholder={t("pages.tasks.taskPayloadPlaceholder")}
                disabled={isEditing || isViewing || isFormDisabled}
                rows={4}
                data-testid="task-form-payload"
              />
              <span className={classes.error}>
                <ErrorMessage name="payload" />
              </span>
            </label>

            <div className={classes.buttonGroup}>
              {!isViewing && (
                <button
                  className={classes.submitButton}
                  type="submit"
                  disabled={isSubmitting || isFormDisabled}
                  data-testid="task-form-submit"
                >
                  {isSubmitting
                    ? isEditing 
                      ? t("pages.tasks.updating")
                      : t("pages.tasks.creating")
                    : isEditing
                    ? t("pages.tasks.updateTask")
                    : t("pages.tasks.createTask")
                  }
                </button>
              )}

              {(isEditing || isViewing) && (
                <button
                  className={classes.cancelButton}
                  type="button"
                  onClick={() => {
                    onCancel();
                    resetForm();
                  }}
                  data-testid="task-form-cancel"
                >
                  {isViewing ? t("pages.tasks.close") : t("pages.tasks.cancel")}
                </button>
              )}
            </div>
          </Form>
        )}
      </Formik>
    </div>
  );
};