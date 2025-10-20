import { useMemo, useState } from "react";
import {
  Box,
  Button,
  Checkbox,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  IconButton,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import AddIcon from "@mui/icons-material/Add";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import RadioButtonUncheckedIcon from "@mui/icons-material/RadioButtonUnchecked";

import {
  useGetWordCards,
  useSetWordCardLearned,
  useCreateWordCard,
  type CreateWordCardRequest,
  type WordCard,
} from "../../api";

import { useStyles } from "./style";

const mockData: WordCard[] = [
  { cardId: "1", hebrew: "שלום", english: "hello", isLearned: false },
  { cardId: "2", hebrew: "תודה", english: "thank you", isLearned: true },
  { cardId: "3", hebrew: "אור", english: "light", isLearned: false },
  { cardId: "4", hebrew: "מים", english: "water", isLearned: false },
  { cardId: "5", hebrew: "ספר", english: "book", isLearned: true },
  { cardId: "6", hebrew: "אוכל", english: "food", isLearned: false },
];

export const WordCards = () => {
  const classes = useStyles();
  const { data, isLoading, isError } = useGetWordCards();
  const [hideLearned, setHideLearned] = useState<boolean>(false);
  const [addOpen, setAddOpen] = useState<boolean>(false);

  // need to change from "mockData" to "data" when api is ready
  const filtered = useMemo(() => {
    const list = mockData ?? [];
    return hideLearned ? list.filter((c) => !c.isLearned) : list;
  }, [hideLearned]);

  return (
    <Box>
      {/* Header */}
      <Box className={classes.headerWrapper}>
        <Typography className={classes.title}>Word Cards</Typography>
        <Typography className={classes.description}>
          <strong>
            Build your own word list and practice Hebrew–English vocabulary.
          </strong>
        </Typography>
        <Typography className={classes.description}>
          Add new words, review your collection, and mark the ones you’ve
          already learned.
        </Typography>

        <Typography className={classes.helperNote}>
          <strong>Tip:</strong> You can also{" "}
          <strong>select any Hebrew word on the site</strong>. A small popup
          will let you add it directly to your cards.
        </Typography>

        <Box className={classes.headerActions}>
          <FormControlLabel
            control={
              <Checkbox
                checked={hideLearned}
                onChange={(e) => setHideLearned(e.target.checked)}
              />
            }
            label="Hide learned"
          />
          <Button
            startIcon={<AddIcon />}
            variant="contained"
            className={classes.primaryBtn}
            onClick={() => setAddOpen(true)}
          >
            Add Card
          </Button>
        </Box>
      </Box>

      {/* Body */}
      <Box className={classes.body}>
        {isLoading && !mockData ? (
          <Box className={classes.centerState}>
            <CircularProgress />
            <Typography>Loading cards…</Typography>
          </Box>
        ) : isError ? (
          <Box className={classes.centerState}>
            <Typography color="error">Failed to load word cards.</Typography>
          </Box>
        ) : filtered.length === 0 ? (
          <Box className={classes.centerState}>
            <Typography>No word cards yet.</Typography>
            <Typography variant="body2" color="text.secondary">
              Click “Add Card” or select a Hebrew word anywhere in the app to
              create one.
            </Typography>
          </Box>
        ) : (
          <Box className={classes.grid}>
            {filtered.map((card) => (
              <WordCardItem key={card.cardId} card={card} />
            ))}
          </Box>
        )}
      </Box>

      {/* Add Dialog */}
      {addOpen && (
        <AddCardDialog open={addOpen} onClose={() => setAddOpen(false)} />
      )}
    </Box>
  );
};

const WordCardItem = ({ card }: { card: WordCard }) => {
  const classes = useStyles();
  const setLearned = useSetWordCardLearned();

  const toggle = () => {
    setLearned.mutate({ cardId: card.cardId, isLearned: !card.isLearned });
  };

  return (
    <Box className={classes.card}>
      <Box className={classes.cardTop}>
        <Box className={classes.wordGroup}>
          <Typography className={classes.hebrew}>{card.hebrew}</Typography>
          <Typography className={classes.english}>{card.english}</Typography>
        </Box>

        <Tooltip
          title={card.isLearned ? "Mark as unlearned" : "Mark as learned"}
        >
          <IconButton
            onClick={toggle}
            className={classes.learnBtn}
            disabled={setLearned.isPending}
            aria-label="toggle learned"
          >
            {card.isLearned ? (
              <CheckCircleIcon className={classes.learnIconActive} />
            ) : (
              <RadioButtonUncheckedIcon className={classes.learnIconIdle} />
            )}
          </IconButton>
        </Tooltip>
      </Box>

      <Box className={classes.cardFoot}>
        <FormControlLabel
          control={
            <Checkbox
              checked={card.isLearned}
              onChange={toggle}
              disabled={setLearned.isPending}
            />
          }
          label="Learned"
        />
      </Box>
    </Box>
  );
};

type AddCardDialogProps = {
  open: boolean;
  onClose: () => void;
};

const AddCardDialog = ({ open, onClose }: AddCardDialogProps) => {
  const classes = useStyles();
  const createCard = useCreateWordCard();

  const [hebrew, setHebrew] = useState<string>("");
  const [english, setEnglish] = useState<string>("");

  const disabled =
    hebrew.trim().length === 0 ||
    english.trim().length === 0 ||
    createCard.isPending;

  const handleSave = () => {
    const body: CreateWordCardRequest = {
      hebrew: hebrew.trim(),
      english: english.trim(),
    };
    createCard.mutate(body, {
      onSuccess: () => {
        setHebrew("");
        setEnglish("");
        onClose();
      },
    });
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="xs"
      fullWidth
      slotProps={{ paper: { className: classes.dialogPaper } }}
    >
      <DialogTitle className={classes.dialogTitle}>
        Add Card
        <IconButton
          onClick={onClose}
          aria-label="close"
          className={classes.closeButton}
        >
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <Box className={classes.dialogBodyGradient}>
          <TextField
            label="Hebrew"
            fullWidth
            autoFocus
            value={hebrew}
            className={classes.textField}
            onChange={(e) => setHebrew(e.target.value)}
            placeholder="הכנס מילה בעברית…"
            inputProps={{ dir: "rtl" }}
            sx={{ mb: 1.5 }}
          />
          <TextField
            label="English"
            fullWidth
            value={english}
            className={classes.textField}
            onChange={(e) => setEnglish(e.target.value)}
            placeholder="Enter the English meaning…"
          />
        </Box>
      </DialogContent>
      <DialogActions className={classes.actions}>
        <Button
          onClick={onClose}
          variant="text"
          className={classes.cancelButton}
        >
          Cancel
        </Button>
        <Button
          onClick={handleSave}
          variant="contained"
          disabled={disabled}
          className={classes.saveButton}
        >
          {createCard.isPending ? "Saving…" : "Save"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
