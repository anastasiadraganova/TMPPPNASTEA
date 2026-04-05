import { apiClient } from '@/lib/api';
import {
  CreateProjectRequest,
  CreateReviewRequest,
  FreelancerPortfolioDto,
  LoginRequest,
  ProjectCategoryNode,
  ProjectDto,
  ProposalDto,
  RegisterRequest,
  ReviewDto,
  UpdateProjectRequest,
  UserDto,
} from '@/types';

const TOKEN_KEY = 'token';

/**
 * Facade для UI-слоя: страницы и store работают с бизнес-операциями,
 * не зная о маршрутах, HTTP-методах и заголовках авторизации.
 */
class ApiFacade {
  private setToken(token: string) {
    if (typeof window !== 'undefined') {
      localStorage.setItem(TOKEN_KEY, token);
    }
  }

  private getToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem(TOKEN_KEY);
  }

  private clearToken() {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(TOKEN_KEY);
    }
  }

  getStoredToken() {
    return this.getToken();
  }

  async login(payload: LoginRequest): Promise<UserDto> {
    const auth = await apiClient.login(payload);
    this.setToken(auth.token);
    return auth.user;
  }

  async register(payload: RegisterRequest): Promise<UserDto> {
    const auth = await apiClient.register(payload);
    this.setToken(auth.token);
    return auth.user;
  }

  async loadCurrentUser(): Promise<UserDto | null> {
    const token = this.getToken();
    if (!token) return null;

    try {
      return await apiClient.getMe();
    } catch {
      this.clearToken();
      return null;
    }
  }

  logout() {
    this.clearToken();
  }

  async getProjects(status?: string, maxBudget?: number): Promise<ProjectDto[]> {
    return apiClient.getProjects(status, maxBudget);
  }

  async getProjectById(id: string): Promise<ProjectDto> {
    return apiClient.getProject(id);
  }

  async getProjectWithProposals(projectId: string, includeProposals: boolean): Promise<{
    project: ProjectDto;
    proposals: ProposalDto[];
  }> {
    const [project, proposals] = await Promise.all([
      apiClient.getProject(projectId),
      includeProposals ? apiClient.getProjectProposals(projectId).catch(() => []) : Promise.resolve([]),
    ]);

    return { project, proposals };
  }

  async createProject(payload: CreateProjectRequest): Promise<ProjectDto> {
    return apiClient.createProject(payload);
  }

  async createProjectFromTemplate(templateId: string): Promise<ProjectDto> {
    return apiClient.createProjectFromTemplate(templateId);
  }

  async updateProject(id: string, payload: UpdateProjectRequest): Promise<ProjectDto> {
    return apiClient.updateProject(id, payload);
  }

  async deleteProject(id: string): Promise<void> {
    return apiClient.deleteProject(id);
  }

  async completeProject(id: string): Promise<void> {
    return apiClient.completeProject(id);
  }

  async assignFreelancer(projectId: string, freelancerId: string): Promise<void> {
    return apiClient.assignFreelancer(projectId, freelancerId);
  }

  async createProposal(projectId: string, coverLetter: string, bidAmount: number): Promise<ProposalDto> {
    return apiClient.createProposal(projectId, { coverLetter, bidAmount });
  }

  async acceptProposal(id: string): Promise<void> {
    return apiClient.acceptProposal(id);
  }

  async rejectProposal(id: string): Promise<void> {
    return apiClient.rejectProposal(id);
  }

  async createReview(payload: CreateReviewRequest): Promise<ReviewDto> {
    return apiClient.createReview(payload);
  }

  async getUserReviews(userId: string): Promise<ReviewDto[]> {
    return apiClient.getUserReviews(userId);
  }

  async getFreelancerPortfolio(userId: string): Promise<FreelancerPortfolioDto> {
    return apiClient.getFreelancerPortfolio(userId);
  }

  async getProjectCategoriesTree(): Promise<ProjectCategoryNode[]> {
    return apiClient.getProjectCategoriesTree();
  }
}

export const apiFacade = new ApiFacade();
