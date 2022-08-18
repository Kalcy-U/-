#define _CRT_SECURE_NO_WARNINGS
#include <cstring>
#include <fstream>
#include <iostream>
#include <map>
#include <queue>
#include <time.h>
#include <random>
using namespace std;

//ȫ�ֱ���
string goal; //Ŀ������
int GoalPos[9];//GoalPos[i]=j��ʾĿ������������i���±�Ϊj
int IntGoal;//int��ʽ��Ŀ������
int (*hfun)(const char *const s); //��������
const string MethodName[]= { "ManhattanDistance" , "NotAtPos" ,"LinearDistance","ManhattanDistance_with0","NotAtPos_with0"};

const int position[9][4] = {
	{1, 3, -1, -1}, // 0�ڵ�0��λ�ÿ��Ժ�λ��1 3����
	{0, 2, 4, -1},	// 1
	{1, 5, -1, -1},
	{0, 6, 4, -1},
	{3, 5, 1, 7},
	{2, 8, 4, -1},
	{3, 7, -1, -1}, // 6
	{6, 8, 4, -1},
	{7, 5, -1, -1}};

//��������
//����char�ַ����е��ַ�
int cpos(char *const s, char c)
{
	if (s != NULL)
	{
		char *p = s;
		while (*p != 0)
		{
			if (*p == c)
				return p - s;
			else
				p++;
		}
	}
	return -1;
}

int LinearDistance(const char *const s)
{
	int count = 0;
	//���Ծ���֮��
	for (int i = 0; i < 9; i++)
	{
		if (s[i] != '0')
			count += abs(GoalPos[s[i] - '0'] - i);
	}
	return count;
}
int ManhattanDistance(const char* const s)
{
	int count = 0;
	//�����پ���֮��
	for (int i = 0; i < 9; i++)
	{
		if (s[i] != '0') {
			int gpos = GoalPos[s[i] - '0'];
			count += abs(gpos / 3 - i / 3) + abs(gpos % 3 - i % 3);
		}
	}
	return count;
}
int ManhattanDistance_with0(const char* const s)
{
	int count = 0;
	//�����پ���֮�� ��0
	for (int i = 0; i < 9; i++)
	{
		int gpos = GoalPos[s[i] - '0'];
		count += abs(gpos / 3 - i / 3) + abs(gpos % 3 - i % 3);
	}
	return count;
}
int NotAtPos_with0(const char *const s)
{
	//����λ���������� ��0
	int count = 0;
	for (int i = 0; i < 9; i++)
	{
		if (s[i] != goal[i])
			count++;
	}
	return count;
}
int NotAtPos(const char* const s)
{
	//����λ����������
	int count = 0;
	for (int i = 0; i < 9; i++)
	{
		if (s[i]!='0'&&s[i] != goal[i])
			count++;
	}
	return count;
}

bool reverse_pair(const string &s1, const string &s2)
{
	if (s1.length() != 9 || s2.length() != 9)
		return false;

	int count = 0;
	for (int i = 0; i < 9; i++)
	{
		if (s1[i] == '0')
			continue;
		for (int j = 0; j < i; j++)
		{
			if (s1[j] > s1[i])
				count++;
		}
	}
	for (int i = 0; i < 9; i++)
	{
		if (s2[i] == '0')
			continue;
		for (int j = 0; j < i; j++)
		{
			if (s2[j] > s2[i])
				count++;
		}
	}
	return (count % 2 == 0);
}

class Node
{
public:
	int sequence; //��ǰ���б��
	int cost;	  // f=g+h�����ۼ�����
	int Parent;	  //��˭��չ����
	int step;
	bool color;

	Node(const char *const s, int stp, int prt)
	{
		sequence = atoi(s);
		step = stp;
		Parent = prt;
		cost = step + hfun(s); //�ⲿ�������ַ������ڲ��洢��int�������������ַ���������������ֵ����
		color = false;
	}
	void reset(const char *const s, int stp, int prt)
	{
		sequence = atoi(s);
		step = stp;
		Parent = prt;
		cost = step + hfun(s);
	}
};
struct cmp
{
	bool operator()(Node *&a, Node *&b) const
	{
		return a->cost > b->cost;
	}
};

class Solve8Num
{
protected:
	int init;
	char initstr[10];
	
public:
	string Method;
	time_t spend_time;
	int MergedNode;
	int CheckedNode;
	bool solvable;
	float Bfactor;
	vector<int> result;
	Solve8Num(const string &s, const string &g, const string &method);
	void Search();
	float BFactor(int N, int d);
};

Solve8Num::Solve8Num(const string &s, const string &g, const string &method)
{
	strncpy(initstr, s.c_str(), 9);
	init = atoi(s.c_str());
	goal = g;
	IntGoal = atoi(g.c_str());
	for (int i = 0; i < 9; i++) {
		GoalPos[goal[i]-'0'] = i;
	}
	solvable = reverse_pair(s, g);
	spend_time = 0;
	MergedNode = 0;
	CheckedNode = 0;
	Bfactor = 0;
	Method = method;
	if (method == "ManhattanDistance")
		hfun = ManhattanDistance;
	else if (method == "LinearDistance")
		hfun = LinearDistance;
	else if (method == "ManhattanDistance_with0")
		hfun = ManhattanDistance_with0;
	else if (method == "NotAtPos_with0")
		hfun = NotAtPos_with0;
	else if (method == "NotAtPos")
		hfun = NotAtPos;
	else {
		cout << "?";
		hfun = ManhattanDistance;
		Method = "ManhattanDistance(default)";
	}
	return;
}

void Solve8Num::Search()
{
	spend_time = clock();
	priority_queue<Node *, vector<Node *>, cmp> que; // f��С���С�
	map<int, Node *> table;							 //�����̽���Ľڵ�
	char s[10];
	char scpy[10];
	int pos0;
	char buffer[50];
	//���ڵ�
	Node *root = new Node(initstr, 0, 0); //��ʼ�ڵ��parent��Ϊ0
	que.push(root);
	table[init] = root;

	MergedNode = 0;
	CheckedNode = 0;
	bool flag = 0;
	while (!que.empty())
	{
		//�����������
		
		Node *check = que.top();
		que.pop();
		
		CheckedNode++;
		if (check->sequence == IntGoal)
		{
			//�ҵ�Ŀ��
			flag = 1;
			break;
		}
		else
		{
			//תchar��������
			sprintf(s, "%09d", check->sequence);
			pos0 = cpos(s, '0');
			
			for (int i = 0; i < 4; i++)
			{
				int temp = position[pos0][i];
				strcpy(scpy, s);
				if (temp == -1)
					break;
				swap(scpy[pos0], scpy[temp]);
				
				int IntNext = atoi(scpy);
				if (table.count(IntNext) == 0)
				{
					Node *next = new Node(scpy, check->step + 1, check->sequence);
					table[IntNext] = next; //����map
					que.push(next);		   //�������
					MergedNode++;
					sprintf(buffer, "\"N%d\"-> \"N%d\"; \n", next->Parent, next->sequence);
				}
			}
		}
	}
	if (flag == 0)
	{
		cout << "notfind";
		return;
	}
	//�ҵ�Ŀ�꣬����·��
	Node *p = table[IntGoal];
	result.push_back(IntGoal);
	while (p->Parent != 0)
	{
		result.push_back(p->Parent);
		p->color = true;
		p = table[p->Parent];
	}
	p->color = true;
	//��תresult
	int size = result.size();
	for (int i = 0; i < size / 2; i++)
	{
		swap(result[i], result[size - 1 - i]);
	}
	spend_time = clock() - spend_time;
	Bfactor = BFactor(MergedNode, size - 1);
	return;
}
bool checkinput(const char *str)
{
	bool checkbox[9] = {0};
	if (str != NULL && strlen(str) == 9)
	{
		for (int i = 0; i < 9; i++)
		{
			if (str[i] >= '0' && str[i] <= '8')
			{
				checkbox[str[i] - '0'] = true;
			}
		}
		for (int i = 0; i < 9; i++)
		{
			if (checkbox[i] == 0)
				return false;
		}
		return true;
	}
	return false;
}
//�������һ�����⣬���ؿɽ���
int randomProblem(string &src,string&dst) {
	src = "";
	dst = "";
	char c;
	srand(time(NULL));
	for (int i = 0; i < 9; i++) {
		do {
			c = rand() % 9 + '0';
		} while (src.find(c) != string::npos);
		src += c;
		//cout << src<<' ';
	}
	for (int i = 0; i++; i < 9) {
		do {
			c = rand() % 9 + '0';
		} while (dst.find(c) != string::npos);
		dst += c;
	}
	return reverse_pair(src, dst);
}

//������ɿɽ�����,���ѽ���Աȱ�������path·���µ�8Nums_MultipelSolution.dat�ļ���
//methodָ�������㷨��method="*"�������������㷨
int MultipelSolution(string src,string dst,const string &path)
{
	ofstream Fout(path + "\\8Nums_MultipelSolution.dat", ios::out);
	
	
	if (!Fout.is_open())
		return -1;
		Fout << "|" << src << "->" << dst << "|����|��չ�ڵ�|���ɽڵ�|��֧����|ʱ��|";
		
		Fout << endl << "|";
		for (int i = 0; i <= 5; i++) {
			Fout << ":-:|";
		}
		Fout << endl;
		for (int i = 0; i < 5; i++) {
			Solve8Num puzzle(src, dst, MethodName[i]);
			puzzle.Search();
			Fout << "|" << puzzle.Method << "|" << puzzle.result.size() - 1 << "|" << puzzle.CheckedNode << "|" << puzzle.MergedNode << "|" << puzzle.Bfactor << "|" << puzzle.spend_time << "ms|" << endl;
		}
		Fout << endl;
	
	Fout.close();
}

float Solve8Num::BFactor(int N, int d) {
	float a = 1, b = 3 ,mid;
	while (b - a > 1e-3f) {
		mid = (a + b) / 2;
		if (pow(mid, d + 1) - mid * (N + 1) + N > 0)
			b = mid;
		else
			a = mid;
	}
	return mid;
}

#if 1
int main(int argc, char **argv)
{
	ofstream file("step.dat", ios::out);
	char str[11];
	if (argc == 4)
	{

		if (checkinput(argv[1]) && checkinput(argv[2]))
		{
			// Solve8Num puzzle("235106487", "123405678");
			Solve8Num puzzle(argv[1], argv[2], argv[3]);
			if (!puzzle.solvable)
			{
				file << "unsolvable" << endl;
				return 1001;
			}
			//cout << LinearDistans(argv[1]) << " " << ManhattanDistance(argv[1]) << endl;
			puzzle.Search();
			file << "solved wiht "<<puzzle.Method << endl;
			file<<"steps : " << puzzle.result.size() - 1 << endl;
			file << "merged nodes : " << puzzle.MergedNode<< endl;
			file << "checked nodes : " << puzzle.CheckedNode << endl;
			file << "spent time : " << puzzle.spend_time << "ms" << endl;
			file << "branching factor : "<< puzzle.Bfactor<<endl;

			for (std::vector<int>::iterator it = puzzle.result.begin(); it != puzzle.result.end(); it++)
			{
				sprintf(str, "%09d", *it);
				file << str << endl;
			}
		}
		else
		{
			// file << "illegle input" << endl;
			return 7;
		}
	}
	else
		return 2000;

	file.close();
	return 0;
}

//����0������7�Ƿ���2000������������ȷ��1001���ɽ�
#else
int main()
{
	string src, dst;
	Solve8Num puzzle("283104765", "123804765", "d");

	puzzle.Search();
}
#endif