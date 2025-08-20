import {
  Box,
  Typography,
  Paper,
  Stack,
  TextField,
  Button,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { useSignalR } from "@/hooks";
import { usePostTask } from "@/api";
import { useState } from "react";

export const SignalRPage = () => {
  const { t } = useTranslation();
  const { status, userId } = useSignalR();
  const { mutate: postTask, isPending } = usePostTask();

  const [id, setId] = useState<string>("1");
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
    if (!isValid) return;

    postTask(
      { id: idNum, name: name.trim(), payload: payload.trim() },
      {
        onSuccess: () => {
          // reset form after success
          setId("1");
          setName("");
          setPayload("");
        },
      }
    );
  };

  return (
    <>
      <Typography variant="h4" gutterBottom>
        {t("SignalR Testing: ")}
      </Typography>
      <Box sx={{ p: 1 }}>
        SignalR status: <b>{status}</b> <br />
        👤 userId: {userId}
      </Box>

      <Paper
        component="form"
        onSubmit={handleSubmit}
        sx={{ mt: 2, p: 2, maxWidth: 520, display: "block" }}
      >
        <Typography variant="h6" gutterBottom>
          {t("pages.home.createTask", "Create Task")}
        </Typography>

        <Stack spacing={2}>
          <TextField
            label={t("forms.task.id", "Task ID")}
            value={id}
            onChange={(e) => setId(e.target.value)}
            required
            fullWidth
          />

          <TextField
            label={t("forms.task.name", "Task Name")}
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            fullWidth
          />

          <TextField
            label={t("forms.task.payload", "Payload")}
            value={payload}
            onChange={(e) => setPayload(e.target.value)}
            required
            fullWidth
            multiline
            minRows={2}
          />

          <Stack direction="row" spacing={1} justifyContent="flex-end">
            <Button
              variant="contained"
              type="submit"
              disabled={isPending || !isValid || status !== "connected"}
            >
              {isPending
                ? t("common.sending", "Posting…")
                : t("common.send", "Post Task")}
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </>
  );
};
