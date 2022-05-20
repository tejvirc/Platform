// OptionParser.cpp : Defines the entry point for the console application.
// Copyright (c) Thomas Rizos - Part of Rizzla Library
//

#include <string>
#include <sstream>

namespace aristocrat
{
	namespace option
	{

		class Option;

		enum Status
		{
			//! The argument is defined but missing value
			ARG_MISSING_VALUE = -1,

			//! The argument is not set/defined
			ARG_UNDEFINED = 0,

			//! The argument is defined.
			ARG_OK = 1,

		};

		struct Arg
		{
		public:
			enum TypeMask
			{
				Unknown = 0,
				None = 1,
				String = 2,
				Numeric = 4,
				Multiple = 8,
				Required = 16
			};

			Arg() : pNext(nullptr), value(nullptr), type(Unknown), status(ARG_UNDEFINED)
			{
			}

			Arg(TypeMask type_) : pNext(nullptr), value(nullptr), type(type_), status(ARG_UNDEFINED)
			{
			}

			~Arg()
			{
				if (pNext != nullptr) delete pNext;
			}

			size_t NumberOfValues()
			{
				size_t count = 0;
				Arg* pArg = pNext;
				while (pArg != nullptr)
				{
					pArg = pArg->pNext;
					++count;
				}
				return count;
			}

			Status		status;
			TypeMask	type;
			const char* value;
			Arg*		pNext;
		};

		struct Descriptor
		{
			const unsigned index;
			const char* const shortopt;
			const char* const longopt;
			const int type; /* option::Arg::TypeMask */
			const char* help;
		};
		size_t desclen(const Descriptor* descs)
		{
			const Descriptor* pd = descs;
			while (pd->shortopt != 0) pd++;
			return pd - descs;
		}

		class Options
		{
		public:
			Options(const Descriptor usage[], char short_delimiter = '-', char remove_delimiter = '#') : _pDescriptor(&usage[0]), _short_delimiter(short_delimiter), _remove_delimiter(remove_delimiter), _last_error(""), _last_error_arg(nullptr), _last_error_desc(-1), _option_buffer(nullptr), _end_option_buffer(nullptr), _buffer_pos(nullptr)
			{
                _numDescriptors = desclen(_pDescriptor);
                _pDescEnd = (Descriptor*)&_pDescriptor[_numDescriptors - 1];
				_args = new Arg[_numDescriptors];
				for (size_t i = 0; i < _numDescriptors; ++i)
				{
					_args->status = ARG_UNDEFINED;
					_args->type = (Arg::TypeMask) _pDescriptor[i].type;
				}
			}

			virtual ~Options()
			{
				if(_args != nullptr)			delete[] _args;
				if(_option_buffer != nullptr)	delete[] _option_buffer;
				
				_end_option_buffer = nullptr;
			}

			inline bool operator[](int index)
			{
				if (index >= (int)_numDescriptors || index < 0) return false;
				return _args[index].status == ARG_OK;
			}

			bool Parse(char** argv, int argc, int buffer_size = 65535)
			{
				if (_option_buffer == nullptr) // second pass of arguments, append to option buffer
				{
					_option_buffer = new char[buffer_size];
					_end_option_buffer = &_option_buffer[buffer_size - 1];
					_buffer_pos = &_option_buffer[0];
				}

				bool remove_modifier = false;
				char** parg = &argv[0];
				int iArg = 0;
				const Descriptor* pTarget = nullptr;
				while (*parg != nullptr && argc--)
				{
					remove_modifier = false;
					strcpy_s(_buffer_pos, (_end_option_buffer - _buffer_pos) - 1, *parg);
					char* arg = _buffer_pos;
					_buffer_pos += strlen(*parg);
					*_buffer_pos++ = 0;
					if ((_end_option_buffer - _buffer_pos) <= 0)
					{
						_last_error = "out of buffer while parsing command line.";
						_last_error_arg = arg;
						return false;
					}
					if (pTarget == nullptr)
					{

						char* arg_p = arg;
						size_t len = strlen(arg);
						if (len < 2)
						{
							_last_error = "missing option directive.";
							_last_error_arg = arg;
							return false;
						}

						// find assigned argument
						while (*arg_p != 0)
						{
							if (*arg_p == '=')
							{
								*arg_p = 0;
								++arg_p;
								break;
							}
							++arg_p;
						}

						if ((arg[0] == '-' && arg[1] == '-') || (arg[0] == _remove_delimiter && arg[1] == _remove_delimiter))
							pTarget = FindLong(&arg[2]);
						else if (arg[0] == _short_delimiter || arg[0] == _remove_delimiter)
							pTarget = FindShort(&arg[1]);

						if (arg[0] == _remove_delimiter)
							remove_modifier = true;

						if (pTarget == desc_end() || pTarget == nullptr)
						{
							pTarget = nullptr;
							_last_error = "invalid option directive.";
							_last_error_arg = arg;
							return false;
						}

						if ((pTarget->type & Arg::None) == Arg::None)
						{
							if (remove_modifier)
							{
								_args[pTarget->index].status = ARG_UNDEFINED;
							}
							else
							{
								_args[pTarget->index].status = ARG_OK;
							}
							pTarget = nullptr;
						}
						else if (*arg_p != 0)
						{
							if (remove_modifier)
							{
								Rem(pTarget);
							}
							else
							{
								Add(pTarget, arg_p);
								pTarget = nullptr;
							}
						}
					}
					else
					{
						Add(pTarget, arg);
						pTarget = nullptr;
					}

					parg++;
					iArg++;
				}
				if (pTarget != nullptr)
				{
					_args[pTarget->index].status = ARG_MISSING_VALUE;
				}

				return validate();
			}
			
			Arg* GetArgument(int index)
			{
				if (index >= (int)_numDescriptors || index < 0) return nullptr;
				return &_args[index];
			}

			Arg* GetLastArgument(int index)
			{
				if (index >= (int)_numDescriptors || index < 0) return nullptr;
				Arg* a = &_args[index];
				while (a->pNext != nullptr)	a = a->pNext;
				return a;
			}

			bool GetArgument(int index, const char**retval, const char* defaultValue)
			{
				*retval = defaultValue;
				Arg* arg = GetLastArgument(index);
				if (arg == nullptr) return false;
				*retval = arg->value;
				return true;
			}
			bool GetArgument(int index, const char**retval)
			{
				Arg* arg = GetLastArgument(index);
				if (arg == nullptr) return false;
				*retval = arg->value;
				return true;
			}
			bool GetArgument(int index, int& retval)
			{
				Arg* arg = GetLastArgument(index);
				if (arg == nullptr) return false;
				retval = atoi(arg->value);
				return true;
			}
			bool GetArgument(int index, long& retval)
			{
				Arg* arg = GetLastArgument(index);
				if (arg == nullptr) return false;
				retval = atol(arg->value);
				return true;
			}
			const char* GetValue(int index)
			{
				Arg* arg = GetLastArgument(index);
				if (arg == nullptr) return false;
				return arg->value;
			}

			char* error_msg()
			{
				if (_last_error_arg != nullptr)
					sprintf_s(_message, MessageBufferLenght, "%s: %s", _last_error_arg, _last_error);
				else if (_last_error_desc >0)
					sprintf_s(_message, MessageBufferLenght, "--%s or %c%s: %s", _pDescriptor[_last_error_desc].longopt, _short_delimiter, _pDescriptor[_last_error_desc].shortopt, _last_error);
				return _message;
			}

			void PrintHelp()
			{
				printf("Usage: \n");
				for (size_t i = 0; i < _numDescriptors; ++i)
				{
					const Descriptor & desc = _pDescriptor[i];
					printf("\t%s\n", desc.help);
				}
			}
			std::string GetHelp()
			{
				std::stringstream ss;
				ss << "Usage: \n";
				for (size_t i = 0; i < _numDescriptors; ++i)
				{
					const Descriptor & desc = _pDescriptor[i];
					ss << "\t" << desc.help << "\n";
				}
				return ss.str();
			}
		private:

			const Descriptor* FindShort(char* key)
			{
				for (size_t i = 0; i < _numDescriptors; ++i)
				{
					if (strcmp(_pDescriptor[i].shortopt, key) == 0)
						return &_pDescriptor[i];
				}
				return desc_end();
			}

			const Descriptor* FindLong(char* key)
			{
				for (size_t i = 0; i < _numDescriptors; ++i)
				{
					if (strcmp(_pDescriptor[i].longopt, key) == 0) return &_pDescriptor[i];
				}
				return desc_end();
			}

			void Add(const Descriptor * key, char* value)
			{
				Add(&_args[key->index], value);
			}

			void Add(Arg* arg, const char* value)
			{
				if (arg->status == ARG_UNDEFINED)
				{
					arg->status = ARG_OK;
					arg->value = value;
				}
				else
				{
					if(arg->pNext != nullptr)
					{
						Add(arg->pNext, value);
					}
					else
					{
						arg->pNext = new Arg(arg->type);
						Add(arg->pNext, value);
					}
				}
			}
			void Rem(const Descriptor * key)
			{
				Rem(&_args[key->index]);
			}

			void Rem(Arg* arg)
			{
				if (arg->status == ARG_UNDEFINED)
				{
					
				}
				else
				{
					arg->pNext = new Arg(arg->type);
					Add(arg->pNext, "");
					arg->pNext->status = ARG_UNDEFINED;
				}
			}
			inline const Descriptor* desc_begin() { return _pDescriptor; }

			inline const Descriptor* desc_end() { return _pDescEnd; }


		private:
			bool validate()
			{
				for (size_t i = 0; i < _numDescriptors; ++i)
				{
					const Descriptor & desc = _pDescriptor[i];
					Arg& src = _args[desc.index];

					if (src.status != ARG_OK && (desc.type & Arg::Required) == Arg::Required)
					{
						_last_error = "required argument missing.";
						_last_error_desc = (int)i;
						return false;
					}

					if (src.status == ARG_OK && (desc.type & Arg::Numeric) == Arg::Numeric)
					{
						size_t len = src.value==nullptr?0:strlen(src.value);
						int allnumeric = 1;
						for (size_t l = 0; l < len && allnumeric; ++l)
						{
							allnumeric &= (int)(isdigit(src.value[l]) != 0 || src.value[l] =='-');
						}

						if (src.value == nullptr)
						{
							_last_error = "missing numeric value.";
							_last_error_desc = (int)i;
						}
						if (!allnumeric)
						{
							_last_error = "not a numeric value.";
							_last_error_desc = (int)i;
							return false;
						}
					}

					if (src.status == ARG_OK && (desc.type & Arg::Multiple) != Arg::Multiple  && src.NumberOfValues() > 1)
					{
						_last_error = "more than one argument of same name.";
						_last_error_desc = (int)i;
						return false;
					}
				}
				return true;
			}

			const int			MessageBufferLenght = 1024;
			char				_message[1024];
			int					_last_error_desc;
			char*				_last_error_arg;
			char*				_last_error;
			char*				_option_buffer;
			char*				_end_option_buffer;
			char*				_buffer_pos;
			char				_short_delimiter;
			char				_remove_delimiter;
			Arg* _args;
			const Descriptor*	_pDescriptor;
			Descriptor*			_pDescEnd;
			size_t				_numDescriptors;
		};


	};
}