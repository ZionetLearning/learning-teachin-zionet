import {
  Activity,
  AlertCircle,
  BarChart3,
  Bell,
  Book,
  BookOpen,
  Lightbulb,
  MessageSquare,
  Minus,
  Play,
  Plus,
  RefreshCw,
  Settings,
  Target,
  TrendingDown,
  TrendingUp,
  Volume2,
  X,
  Zap,
} from "lucide-react";

import { Badge } from "@ui-shadcn-components/badge";
import { Button } from "@ui-shadcn-components/button";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@ui-shadcn-components/tabs";

type EngagementLevel = "high" | "medium" | "low";
type PersonalizationMode = "Auto" | "Override";

interface StudentData {
  id: number;
  name: string;
  progress: number;
  engagement: EngagementLevel;
  hasAlert: boolean;
  currentTask: string;
  personalizationMode: PersonalizationMode;
}

export interface DrillInPanelProps {
  studentName: string | null;
  selectedStudentIds: number[];
  allStudentIds: number[];
  students: StudentData[];
  onClose: () => void;
}

export const DrillInPanel = ({
  studentName,
  selectedStudentIds,
  allStudentIds,
  students,
  onClose,
}: DrillInPanelProps) => {
  const effectiveSelectedIds =
    selectedStudentIds.length === 0 ? allStudentIds : selectedStudentIds;
  const selectedCount = effectiveSelectedIds.length;
  const selectedStudents = students.filter((student) =>
    effectiveSelectedIds.includes(student.id),
  );

  const avgProgress =
    selectedStudents.length > 0
      ? Math.round(
          selectedStudents.reduce((sum, student) => sum + student.progress, 0) /
            selectedStudents.length,
        )
      : 0;

  const engagementCounts = {
    high: selectedStudents.filter((student) => student.engagement === "high")
      .length,
    medium: selectedStudents.filter(
      (student) => student.engagement === "medium",
    ).length,
    low: selectedStudents.filter((student) => student.engagement === "low")
      .length,
  };

  const alertCount = selectedStudents.filter(
    (student) => student.hasAlert,
  ).length;

  const alertStudents = selectedStudents.filter((student) => student.hasAlert);

  const progressRanges = {
    high: selectedStudents.filter((student) => student.progress >= 75).length,
    medium: selectedStudents.filter(
      (student) => student.progress >= 50 && student.progress < 75,
    ).length,
    low: selectedStudents.filter((student) => student.progress < 50).length,
  };

  return (
    <div className="w-full md:w-96 bg-card border-l-2 border-primary shadow-xl overflow-y-auto flex-shrink-0">
      <div className="sticky top-0 bg-card border-b-2 border-border p-4">
        <div className="flex items-center justify-between mb-2">
          <h2 className="text-lg font-bold">Control Panel</h2>
          {studentName && (
            <Button
              variant="ghost"
              size="sm"
              onClick={onClose}
              title="Clear student selection"
            >
              <X className="w-4 h-4" />
            </Button>
          )}
        </div>
        <p className="text-xs text-muted-foreground">
          Notifications, Student Details & Lesson Controls
        </p>
      </div>

      <Tabs defaultValue="notifications" className="w-full">
        <TabsList className="w-full justify-start border-b-2 border-border rounded-none h-auto p-0">
          <TabsTrigger
            value="notifications"
            className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary"
          >
            <Bell className="w-4 h-4 mr-2" />
            Alerts
          </TabsTrigger>
          <TabsTrigger
            value="student"
            className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary"
          >
            <TrendingDown className="w-4 h-4 mr-2" />
            Student
          </TabsTrigger>
          <TabsTrigger
            value="lesson"
            className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary"
          >
            <Settings className="w-4 h-4 mr-2" />
            Lesson
          </TabsTrigger>
        </TabsList>

        <TabsContent value="notifications" className="p-4 space-y-4">
          <div className="border-2 border-border p-3">
            <h3 className="text-sm font-bold mb-3 flex items-center gap-2">
              <AlertCircle className="w-4 h-4 text-alert" />
              Active Alerts
              {selectedCount > 0 && (
                <span className="text-xs text-muted-foreground font-normal">
                  (
                  {selectedCount === allStudentIds.length
                    ? "All"
                    : selectedCount}{" "}
                  students)
                </span>
              )}
            </h3>
            <div className="space-y-3 text-xs">
              {alertStudents.length === 0 ? (
                <div className="p-2 border border-border text-muted-foreground">
                  No active alerts.
                </div>
              ) : (
                alertStudents.map((student) => (
                  <div key={student.id} className="p-2 border border-alert bg-alert/10">
                    <div className="font-medium flex items-center gap-2">
                      <AlertCircle className="w-4 h-4 text-alert" />
                      {student.name}
                    </div>
                    <div className="text-muted-foreground mt-1">
                      {student.currentTask}
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        </TabsContent>

        <TabsContent value="student" className="p-4 space-y-4">
          <div className="border-2 border-border p-3">
            <h3 className="text-sm font-bold mb-3 flex items-center gap-2">
              <Activity className="w-4 h-4" />
              Status Overview
            </h3>
            <div className="grid grid-cols-2 gap-3 text-xs">
              <div className="p-2 border border-border rounded">
                <div className="text-muted-foreground mb-1">Total Students</div>
                <div className="text-lg font-bold">{selectedCount}</div>
              </div>
              <div className="p-2 border border-alert bg-alert/10 rounded">
                <div className="text-muted-foreground mb-1">Active Alerts</div>
                <div className="text-lg font-bold text-alert">{alertCount}</div>
              </div>
              <div className="p-2 border border-border rounded">
                <div className="text-muted-foreground mb-1">On Track</div>
                <div className="text-lg font-bold text-green-600">
                  {progressRanges.high}
                </div>
              </div>
              <div className="p-2 border border-border rounded">
                <div className="text-muted-foreground mb-1">Needs Support</div>
                <div className="text-lg font-bold text-orange-600">
                  {progressRanges.low}
                </div>
              </div>
            </div>
          </div>

          <div className="border-2 border-border p-3">
            <h3 className="text-sm font-bold mb-3 flex items-center gap-2">
              <BarChart3 className="w-4 h-4" />
              Progress Metrics
            </h3>
            <div className="space-y-3">
              <div>
                <div className="flex justify-between text-xs mb-1">
                  <span className="text-muted-foreground">
                    Average Progress
                  </span>
                  <span className="font-bold">{avgProgress}%</span>
                </div>
                <div className="w-full h-2 bg-wireframe-medium rounded-full overflow-hidden">
                  <div
                    className="h-full bg-primary transition-all rounded-full"
                    style={{ width: `${avgProgress}%` }}
                  />
                </div>
              </div>
              <div className="grid grid-cols-3 gap-2 text-xs">
                <div className="p-2 border border-green-200 bg-green-50 rounded">
                  <div className="text-muted-foreground mb-1">High (75%+)</div>
                  <div className="font-bold text-green-700">
                    {progressRanges.high}
                  </div>
                </div>
                <div className="p-2 border border-yellow-200 bg-yellow-50 rounded">
                  <div className="text-muted-foreground mb-1">
                    Medium (50-74%)
                  </div>
                  <div className="font-bold text-yellow-700">
                    {progressRanges.medium}
                  </div>
                </div>
                <div className="p-2 border border-red-200 bg-red-50 rounded">
                  <div className="text-muted-foreground mb-1">
                    Low (&lt;50%)
                  </div>
                  <div className="font-bold text-red-700">
                    {progressRanges.low}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="border-2 border-border p-3">
            <h3 className="text-sm font-bold mb-3 flex items-center gap-2">
              <TrendingUp className="w-4 h-4" />
              Engagement KPIs
            </h3>
            <div className="space-y-3">
              <div className="flex items-center gap-3">
                <div className="flex gap-px items-center h-3">
                  {[0, 1, 2, 3, 4].map((value) => (
                    <div
                      key={value}
                      className="h-[10px] w-[4px] rounded-[1px] bg-[rgba(22,140,41,0.6)]"
                    />
                  ))}
                </div>
                <div className="flex-1">
                  <div className="flex justify-between text-xs mb-1">
                    <span>High Engagement</span>
                    <span className="font-bold">
                      {engagementCounts.high} (
                      {selectedCount > 0
                        ? Math.round(
                            (engagementCounts.high / selectedCount) * 100,
                          )
                        : 0}
                      %)
                    </span>
                  </div>
                  <div className="w-full h-1.5 bg-wireframe-medium rounded-full overflow-hidden">
                    <div
                      className="h-full bg-green-600 transition-all rounded-full"
                      style={{
                        width: `${selectedCount > 0 ? (engagementCounts.high / selectedCount) * 100 : 0}%`,
                      }}
                    />
                  </div>
                </div>
              </div>

              <div className="flex items-center gap-3">
                <div className="flex gap-px items-center h-3">
                  {[0, 1, 2].map((value) => (
                    <div
                      key={value}
                      className="h-[10px] w-[4px] rounded-[1px] bg-[rgba(255,193,7,0.6)]"
                    />
                  ))}
                </div>
                <div className="flex-1">
                  <div className="flex justify-between text-xs mb-1">
                    <span>Medium Engagement</span>
                    <span className="font-bold">
                      {engagementCounts.medium} (
                      {selectedCount > 0
                        ? Math.round(
                            (engagementCounts.medium / selectedCount) * 100,
                          )
                        : 0}
                      %)
                    </span>
                  </div>
                  <div className="w-full h-1.5 bg-wireframe-medium rounded-full overflow-hidden">
                    <div
                      className="h-full bg-yellow-600 transition-all rounded-full"
                      style={{
                        width: `${selectedCount > 0 ? (engagementCounts.medium / selectedCount) * 100 : 0}%`,
                      }}
                    />
                  </div>
                </div>
              </div>

              <div className="flex items-center gap-3">
                <div className="flex gap-px items-center h-3">
                  <div className="h-[10px] w-[4px] rounded-[1px] bg-[rgba(220,53,69,0.6)]" />
                </div>
                <div className="flex-1">
                  <div className="flex justify-between text-xs mb-1">
                    <span>Low Engagement</span>
                    <span className="font-bold">
                      {engagementCounts.low} (
                      {selectedCount > 0
                        ? Math.round(
                            (engagementCounts.low / selectedCount) * 100,
                          )
                        : 0}
                      %)
                    </span>
                  </div>
                  <div className="w-full h-1.5 bg-wireframe-medium rounded-full overflow-hidden">
                    <div
                      className="h-full bg-red-600 transition-all rounded-full"
                      style={{
                        width: `${selectedCount > 0 ? (engagementCounts.low / selectedCount) * 100 : 0}%`,
                      }}
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="border-2 border-border p-3">
            <h3 className="text-sm font-bold mb-3 flex items-center gap-2">
              <Target className="w-4 h-4" />
              Level Indicators by Aspect
            </h3>
            <div className="space-y-3 text-xs">
              <div>
                <div className="flex justify-between mb-1">
                  <span className="font-medium">Concept Understanding</span>
                  <Badge variant="secondary" className="text-xs">
                    High
                  </Badge>
                </div>
                <div className="flex gap-px items-center h-3 mt-1">
                  {[0, 1, 2, 3, 4].map((value) => (
                    <div
                      key={value}
                      className="h-[10px] w-[4px] rounded-[1px] bg-[rgba(22,140,41,0.6)]"
                    />
                  ))}
                </div>
                <div className="text-muted-foreground mt-1">
                  {selectedCount > 0
                    ? Math.round((progressRanges.high / selectedCount) * 100)
                    : 0}
                  % at mastery level
                </div>
              </div>

              <div>
                <div className="flex justify-between mb-1">
                  <span className="font-medium">Skill Application</span>
                  <Badge variant="secondary" className="text-xs">
                    Medium
                  </Badge>
                </div>
                <div className="flex gap-px items-center h-3 mt-1">
                  {[0, 1, 2].map((value) => (
                    <div
                      key={value}
                      className="h-[10px] w-[4px] rounded-[1px] bg-[rgba(255,193,7,0.6)]"
                    />
                  ))}
                </div>
                <div className="text-muted-foreground mt-1">
                  {selectedCount > 0
                    ? Math.round((progressRanges.medium / selectedCount) * 100)
                    : 0}
                  % developing
                </div>
              </div>

              <div>
                <div className="flex justify-between mb-1">
                  <span className="font-medium">Critical Thinking</span>
                  <Badge variant="secondary" className="text-xs">
                    High
                  </Badge>
                </div>
                <div className="flex gap-px items-center h-3 mt-1">
                  {[0, 1, 2, 3, 4].map((value) => (
                    <div
                      key={value}
                      className="h-[10px] w-[4px] rounded-[1px] bg-[rgba(22,140,41,0.6)]"
                    />
                  ))}
                </div>
                <div className="text-muted-foreground mt-1">
                  {selectedCount > 0
                    ? Math.round((progressRanges.high / selectedCount) * 100)
                    : 0}
                  % at mastery level
                </div>
              </div>

              <div>
                <div className="flex justify-between mb-1">
                  <span className="font-medium">Pacing & Flow</span>
                  <Badge variant="secondary" className="text-xs">
                    Medium
                  </Badge>
                </div>
                <div className="flex gap-px items-center h-3 mt-1">
                  {[0, 1, 2].map((value) => (
                    <div
                      key={value}
                      className="h-[10px] w-[4px] rounded-[1px] bg-[rgba(255,193,7,0.6)]"
                    />
                  ))}
                </div>
                <div className="text-muted-foreground mt-1">
                  {selectedCount > 0
                    ? Math.round((progressRanges.medium / selectedCount) * 100)
                    : 0}
                  % on pace
                </div>
              </div>
            </div>
          </div>

          {studentName && (
            <div className="border-2 border-primary p-3 bg-primary/5">
              <h3 className="text-sm font-bold mb-1">{studentName}</h3>
              <p className="text-xs text-muted-foreground mb-3">
                Individual student view
              </p>
              {(() => {
                const student = selectedStudents.find(
                  (candidate) => candidate.name === studentName,
                );
                if (!student) return null;

                return (
                  <div className="space-y-2 text-xs">
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Progress:</span>
                      <span className="font-bold">{student.progress}%</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Engagement:</span>
                      <Badge
                        variant={
                          student.engagement === "high"
                            ? "default"
                            : student.engagement === "medium"
                              ? "secondary"
                              : "destructive"
                        }
                        className="text-xs capitalize"
                      >
                        {student.engagement}
                      </Badge>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">
                        Current Task:
                      </span>
                      <span className="font-medium">{student.currentTask}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Mode:</span>
                      <Badge variant="secondary" className="text-xs">
                        {student.personalizationMode}
                      </Badge>
                    </div>
                  </div>
                );
              })()}
            </div>
          )}
        </TabsContent>

        <TabsContent value="lesson" className="p-4 space-y-4">
          <div className="border-2 border-accent bg-accent/10 p-3 mb-4">
            <h3 className="text-sm font-bold mb-3">
              Group Actions (
              {selectedCount === allStudentIds.length ? "All" : selectedCount}{" "}
              selected)
            </h3>
            <div className="flex flex-wrap gap-2">
              <Button variant="outline" size="sm">
                <Plus className="w-3 h-3 mr-1" /> Adjust Pace
              </Button>
              <Button variant="outline" size="sm">
                <Minus className="w-3 h-3 mr-1" /> Slow Down
              </Button>
              <Button variant="outline" size="sm">
                <Lightbulb className="w-3 h-3 mr-1" /> Push Hint
              </Button>
              <Button variant="outline" size="sm">
                <MessageSquare className="w-3 h-3 mr-1" /> Request Reflection
              </Button>
              <Button variant="outline" size="sm">
                <BookOpen className="w-3 h-3 mr-1" /> Reteach Concept
              </Button>
              <Button variant="outline" size="sm">
                <Target className="w-3 h-3 mr-1" /> Reinforce Skill
              </Button>
              <Button variant="outline" size="sm">
                <RefreshCw className="w-3 h-3 mr-1" /> Sync with Teacher
              </Button>
              <Button variant="outline" size="sm">
                <Play className="w-3 h-3 mr-1" /> Release to Self-Paced
              </Button>
            </div>
          </div>

          <div className="border-2 border-border p-3">
            <h3 className="text-sm font-bold mb-3 flex items-center gap-2">
              <Zap className="w-4 h-4" />
              Pacing Controls
            </h3>
            <div className="space-y-3">
              <div>
                <label className="text-xs font-medium mb-1 block">
                  Lesson Speed
                </label>
                <div className="grid grid-cols-3 gap-2">
                  <Button variant="outline" size="sm" className="text-xs">
                    Slow
                  </Button>
                  <Button variant="default" size="sm" className="text-xs">
                    Normal
                  </Button>
                  <Button variant="outline" size="sm" className="text-xs">
                    Fast
                  </Button>
                </div>
              </div>
              <div>
                <label className="text-xs font-medium mb-1 block">
                  Time per Activity
                </label>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    className="flex-1 text-xs"
                  >
                    -5 min
                  </Button>
                  <span className="text-sm font-mono self-center">15 min</span>
                  <Button
                    variant="outline"
                    size="sm"
                    className="flex-1 text-xs"
                  >
                    +5 min
                  </Button>
                </div>
              </div>
            </div>
          </div>

          <div className="border-2 border-border p-3">
            <h3 className="text-sm font-bold mb-3 flex items-center gap-2">
              <Book className="w-4 h-4" />
              Content Detail Level
            </h3>
            <div className="space-y-3">
              <div>
                <label className="text-xs font-medium mb-1 block">
                  Explanation Depth
                </label>
                <div className="grid grid-cols-3 gap-2">
                  <Button variant="outline" size="sm" className="text-xs">
                    Brief
                  </Button>
                  <Button variant="default" size="sm" className="text-xs">
                    Standard
                  </Button>
                  <Button variant="outline" size="sm" className="text-xs">
                    Deep
                  </Button>
                </div>
              </div>
              <div>
                <label className="text-xs font-medium mb-1 block">
                  Support Scaffolding
                </label>
                <div className="grid grid-cols-3 gap-2">
                  <Button variant="outline" size="sm" className="text-xs">
                    Low
                  </Button>
                  <Button variant="default" size="sm" className="text-xs">
                    Medium
                  </Button>
                  <Button variant="outline" size="sm" className="text-xs">
                    High
                  </Button>
                </div>
              </div>
            </div>
          </div>

          <div className="border-2 border-border p-3">
            <h3 className="text-sm font-bold mb-3 flex items-center gap-2">
              <Volume2 className="w-4 h-4" />
              Delivery Mode
            </h3>
            <div className="space-y-2">
              <Button
                variant="outline"
                size="sm"
                className="w-full text-xs justify-start"
              >
                Teacher-Led (Synchronous)
              </Button>
              <Button
                variant="default"
                size="sm"
                className="w-full text-xs justify-start"
              >
                Hybrid Mode (Current)
              </Button>
              <Button
                variant="outline"
                size="sm"
                className="w-full text-xs justify-start"
              >
                Self-Paced (Asynchronous)
              </Button>
            </div>
          </div>

          <div className="border-2 border-border p-3">
            <h3 className="text-sm font-bold mb-3">Global Actions</h3>
            <div className="space-y-2">
              <Button variant="outline" size="sm" className="w-full text-xs">
                Pause All Students
              </Button>
              <Button variant="outline" size="sm" className="w-full text-xs">
                All-Eyes-on-Me
              </Button>
              <Button variant="outline" size="sm" className="w-full text-xs">
                Skip Current Activity
              </Button>
              <Button variant="outline" size="sm" className="w-full text-xs">
                Extend Time (+10 min)
              </Button>
            </div>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
};
