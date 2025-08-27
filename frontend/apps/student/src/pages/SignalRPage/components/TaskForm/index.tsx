import { useState } from "react";
import { Paper, Stack, TextField, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { Button } from "../../../../../../../libs/ui/components";
//import { Button } from "@frontend/ui-components/components";
export type TaskInput = { id: number; name: string; payload: string };

type Props = {
  isPending: boolean;
  disabled?: boolean;
  onSubmit: (task: TaskInput, reset: () => void) => void;
  defaultId?: string;
};

export const TaskForm = ({
  isPending,
  disabled,
  onSubmit,
  defaultId = "1",
}: Props) => {
  const { t } = useTranslation();
  const [id, setId] = useState<string>(defaultId);
  const [name, setName] = useState<string>("");
  const [payload, setPayload] = useState<string>("");

  const idNum = Number(id);
  const isValid =
    Number.isFinite(idNum) &&
    idNum > 0 &&
    name.trim().length > 0 &&
    payload.trim().length > 0;

  const handleSubmit: React.FormEventHandler<HTMLFormElement> = (e) => {
    e.preventDefault();
    if (!isValid || disabled) return;

    const reset = () => {
      setId(defaultId);
      setName("");
      setPayload("");
    };

    onSubmit({ id: idNum, name: name.trim(), payload: payload.trim() }, reset);
  };

  return (
    <Paper
      component="form"
      onSubmit={handleSubmit}
      sx={{ mt: 2, p: 2, maxWidth: 520 }}
    >
      <Typography variant="h6" gutterBottom>
        {t("pages.signalR.createTask")}
      </Typography>

      <Stack spacing={2}>
        <TextField
          label={t("pages.signalR.taskId")}
          value={id}
          onChange={(e) => setId(e.target.value)}
          required
          fullWidth
        />

        <TextField
          label={t("pages.signalR.taskName")}
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          fullWidth
        />

        <TextField
          label={t("pages.signalR.payload")}
          value={payload}
          onChange={(e) => setPayload(e.target.value)}
          required
          fullWidth
          multiline
          minRows={2}
        />

        <Stack direction="row" spacing={1} justifyContent="flex-end">
          <Button
            disabled={isPending || disabled || !isValid}
          >
            {isPending ? t("pages.signalR.posting") : t("pages.signalR.send")}
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
};
